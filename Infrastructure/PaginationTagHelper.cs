using Intextwo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Intextwo.Infrastructure
{
    [HtmlTargetElement("div", Attributes = "page-model")]



    public class PaginationTagHelper: TagHelper
    {
        private IUrlHelperFactory urlHelperFactory;

        public PaginationTagHelper(IUrlHelperFactory temp)
        {
            this.urlHelperFactory = temp;
        }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext? ViewContext { get; set; }

        public string? PageAction { get; set; }
        public PaginationInfo PageModel { get; set; }

        public bool PageClassEnabled { get; set; } = false;
        public string PageClass { get; set; } = string.Empty;
        public string PageClassNormal { get; set;} = string.Empty;
        public string PageClassSelected { get; set; } = string.Empty;


        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (ViewContext != null && PageModel != null)
            {
                IUrlHelper urlHelper = urlHelperFactory.GetUrlHelper(ViewContext);
                TagBuilder result = new TagBuilder("div");

                // Define the start and end page numbers for the loop
                int startPage = Math.Max(PageModel.CurrentPage - 3, 1);
                int endPage = Math.Min(PageModel.CurrentPage + 3, PageModel.TotalPages);

                if (startPage > 1)
                {
                    // Create a button/link for the first page
                    result.InnerHtml.AppendHtml(CreatePageLink(1, urlHelper));
                    if (startPage > 2)
                    {
                        // Add an ellipsis if there is a gap between the first page and the current page range
                        result.InnerHtml.AppendHtml(CreateEllipsis());
                    }
                }

                for (int i = startPage; i <= endPage; i++)
                {
                    // Create a button/link for each page number in the range
                    result.InnerHtml.AppendHtml(CreatePageLink(i, urlHelper));
                }

                if (endPage < PageModel.TotalPages)
                {
                    if (endPage < PageModel.TotalPages - 1)
                    {
                        // Add an ellipsis if there is a gap between the current page range and the last page
                        result.InnerHtml.AppendHtml(CreateEllipsis());
                    }
                    // Create a button/link for the last page
                    result.InnerHtml.AppendHtml(CreatePageLink(PageModel.TotalPages, urlHelper));
                }

                output.Content.AppendHtml(result.InnerHtml);
            }
        }
        private TagBuilder CreatePageLink(int pageNumber, IUrlHelper urlHelper)
        {
            TagBuilder tag = new TagBuilder("a");
            tag.Attributes["href"] = urlHelper.Action(PageAction, new { pageNum = pageNumber });

            if (PageClassEnabled)
            {
                tag.AddCssClass(PageClass);
                tag.AddCssClass(pageNumber == PageModel.CurrentPage ? PageClassSelected : PageClassNormal);
            }

            tag.InnerHtml.Append(pageNumber.ToString());
            return tag;
        }
        private TagBuilder CreateEllipsis()
        {
            TagBuilder tag = new TagBuilder("span"); // Use 'span' since it's not a clickable link
            if (PageClassEnabled)
            {
                tag.AddCssClass(PageClass);
                tag.AddCssClass(PageClassNormal); // Only the normal class, not the selected class
            }
            tag.InnerHtml.Append("..."); // Add the ellipsis text
            return tag;
        }

    }
}
