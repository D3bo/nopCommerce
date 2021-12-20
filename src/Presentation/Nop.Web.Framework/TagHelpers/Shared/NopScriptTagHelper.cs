﻿using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Web.Framework.Configuration;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.UI;
using WebOptimizer;
using WebOptimizer.Extensions;

namespace Nop.Web.Framework.TagHelpers.Shared
{
    /// <summary>
    /// Script bundling tag helper
    /// </summary>
    [HtmlTargetElement(SCRIPT_TAG_NAME)]
    [HtmlTargetElement(BUNDLE_TAG_NAME)]
    public class NopScriptTagHelper : UrlResolutionTagHelper
    {
        #region Constants

        private const string BUNDLE_TAG_NAME = "script-bundle";
        private const string SCRIPT_TAG_NAME = "script";

        private const string BUNDLE_DESTINATION_KEY_NAME = "asp-bundle-dest-key";
        private const string BUNDLE_KEY_NAME = "asp-bundle-key";
        private const string EXCLUDE_FROM_BUNDLE_ATTRIBUTE_NAME = "asp-exclude-from-bundle";
        private const string DEBUG_SRC_ATTRIBUTE_NAME = "asp-debug-src";
        private const string LOCATION_ATTRIBUTE_NAME = "asp-location";
        private const string SRC_ATTRIBUTE_NAME = "src";

        #endregion

        #region Fields

        private readonly AppSettings _appSettings;
        private readonly IAssetPipeline _assetPipeline;
        private readonly INopHtmlHelper _nopHtmlHelper;
        private readonly IWebHostEnvironment _webHostEnvironment;

        #endregion

        #region Ctor

        public NopScriptTagHelper(AppSettings appSettings,
            HtmlEncoder htmlEncoder,
            IAssetPipeline assetPipeline,
            INopHtmlHelper nopHtmlHelper,
            IUrlHelperFactory urlHelperFactory,
            IWebHostEnvironment webHostEnvironment) : base(urlHelperFactory, htmlEncoder)
        {
            _appSettings = appSettings;
            _assetPipeline = assetPipeline ?? throw new ArgumentNullException(nameof(assetPipeline));
            _nopHtmlHelper = nopHtmlHelper;
            _webHostEnvironment = webHostEnvironment;
        }

        #endregion

        #region Utils

        private static async Task<string> BuildInlineScriptTagAsync(TagHelperOutput output)
        {
            //get JavaScript
            var scriptTag = new TagBuilder(SCRIPT_TAG_NAME);

            var childContent = await output.GetChildContentAsync();
            var script = childContent.GetContent();

            if (!string.IsNullOrEmpty(script))
                scriptTag.InnerHtml.SetHtmlContent(new HtmlString(script));

            scriptTag.MergeAttributes(await output.GetAttributeDictionaryAsync(), replaceExisting: false);

            return await scriptTag.RenderHtmlContentAsync() + Environment.NewLine;
        }

        private void ProcessSrcAttribute(TagHelperContext context, TagHelperOutput output)
        {
            if (!string.IsNullOrEmpty(DebugSrc) && _webHostEnvironment.IsDevelopment())
                Src = DebugSrc;

            // Pass through attribute that is also a well-known HTML attribute.
            if (Src != null)
                output.CopyHtmlAttribute(SRC_ATTRIBUTE_NAME, context);

            // If there's no "src" attribute in output.Attributes this will noop.
            ProcessUrlAttribute(SRC_ATTRIBUTE_NAME, output);

            // Retrieve the TagHelperOutput variation of the "src" attribute in case other TagHelpers in the
            // pipeline have touched the value. If the value is already encoded this ScriptTagHelper may
            // not function properly.
            if (output.Attributes[SRC_ATTRIBUTE_NAME]?.Value is string srcAttribute)
                Src = srcAttribute;
        }

        private void ProcessAsset(TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(Src))
                return;

            //remove the application path from the generated URL if exists
            var pathBase = ViewContext.HttpContext?.Request?.PathBase ?? PathString.Empty;
            PathString.FromUriComponent(Src).StartsWithSegments(pathBase, out var sourceFile);

            if (!_assetPipeline.TryGetAssetFromRoute(sourceFile, out var asset))
            {
                asset = _assetPipeline.AddJavaScriptBundle(sourceFile, sourceFile);
            }

            output.Attributes.SetAttribute(SRC_ATTRIBUTE_NAME, $"{Src}?v={asset.GenerateCacheKey(ViewContext.HttpContext)}");
        }

        private string GetBundleSuffix()
        {
            var bundleSuffix = _appSettings.Get<WebOptimizerConfig>().JavaScriptBundleSuffix;

            //to avoid collisions in controllers with the same names
            if (ViewContext.RouteData.Values.TryGetValue("area", out var area))
                bundleSuffix = $"{bundleSuffix}.{area}".ToLowerInvariant();

            return bundleSuffix;
        }

        #endregion

        #region Methods

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (output == null)
                throw new ArgumentNullException(nameof(output));

            var config = _appSettings.Get<WebOptimizerConfig>();

            if (config.EnableTagHelperBundling != true && string.Equals(context.TagName, BUNDLE_TAG_NAME))
            {
                // do not show bundle tag
                output.SuppressOutput();
                return;
            }

            output.TagName = SCRIPT_TAG_NAME;
            output.TagMode = TagMode.StartTagAndEndTag;

            if (!output.Attributes.ContainsName("type")) // we don't touch other types e.g. text/template
                output.Attributes.SetAttribute("type", MimeTypes.TextJavascript);

            ProcessSrcAttribute(context, output);

            if (Location == ResourceLocation.Pinned)
            {
                if (string.IsNullOrEmpty(Src))
                    output.Content.SetHtmlContent(await BuildInlineScriptTagAsync(output));
                else
                    ProcessAsset(output);
                return;
            }

            //bundling
            if (config.EnableJavaScriptBundling)
            {
                if (config.EnableTagHelperBundling == true)
                {
                    var defaultBundleBuffix = GetBundleSuffix();
                    if (string.Equals(context.TagName, BUNDLE_TAG_NAME))
                    {
                        output.HandleJsBundle(_assetPipeline, ViewContext, config, Src, string.Empty, BundleDestinationKey ?? defaultBundleBuffix);
                        return;
                    }

                    if (!string.IsNullOrEmpty(Src) && !ExcludeFromBundle)
                    {
                        output.HandleJsBundle(_assetPipeline, ViewContext, config, Src, BundleKey ?? defaultBundleBuffix, string.Empty);
                        return;
                    }
                }

                if (Location == ResourceLocation.Auto) // move to the footer bundle
                    Location = ResourceLocation.Footer;
            }
            else
            {
                ProcessAsset(output);
            }

            var tagHtml = await BuildInlineScriptTagAsync(output);

            if (Location == ResourceLocation.Auto)
            {
                if (string.IsNullOrEmpty(Src))
                    output.Content.SetHtmlContent(tagHtml);
                else
                    ProcessAsset(output);
                return;
            }

            if (string.IsNullOrEmpty(Src))
                _nopHtmlHelper.AddInlineScriptParts(Location, tagHtml);
            else
                _nopHtmlHelper.AddScriptParts(Location, Src, DebugSrc, ExcludeFromBundle);

            output.SuppressOutput();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Script path (e.g. full debug version). If empty, then minified version will be used
        /// </summary>
        [HtmlAttributeName(DEBUG_SRC_ATTRIBUTE_NAME)]
        public string DebugSrc { get; set; }

        /// <summary>
        /// Indicates where the script should be rendered
        /// </summary>
        [HtmlAttributeName(LOCATION_ATTRIBUTE_NAME)]
        public ResourceLocation Location { set; get; }

        /// <summary>
        /// A value indicating if a file should be excluded from the bundle
        /// </summary>
        [HtmlAttributeName(EXCLUDE_FROM_BUNDLE_ATTRIBUTE_NAME)]
        public bool ExcludeFromBundle { get; set; }

        /// <summary>
        /// A key of a bundle to collect
        /// </summary>
        [HtmlAttributeName(BUNDLE_KEY_NAME)]
        public string BundleKey { get; set; }

        /// <summary>
        /// A key that defines the destination for the bundle.
        /// </summary>
        [HtmlAttributeName(BUNDLE_DESTINATION_KEY_NAME)]
        public string BundleDestinationKey { get; set; }

        /// <summary>
        /// Address of the external script to use
        /// </summary>
        [HtmlAttributeName(SRC_ATTRIBUTE_NAME)]
        public string Src { get; set; }

        #endregion

    }
}