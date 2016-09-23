using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

using PickemApp.Controllers;

namespace PickemApp.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString DisplayMessage(this HtmlHelper htmlHelper, AccountController.ManageMessageId? messageId, string messageType = "success")
        {
            string message = "";
            switch (messageId)
            {
                case AccountController.ManageMessageId.ResetPasswordSuccess:
                    message = "Your password was reset.";
                    break;
                case AccountController.ManageMessageId.ForgotPasswordSuccess:
                    message = "An email was sent with instructions on how to reset your password.";
                    break;
            }

            var pTag = new TagBuilder("p");
            pTag.AddCssClass("alert alert-dismissable alert-" + messageType);

            // <button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>

            var buttonTag = new TagBuilder("button");
            buttonTag.AddCssClass("close");
            buttonTag.MergeAttribute("type", "button");
            buttonTag.MergeAttribute("data-dismiss", "alert");
            buttonTag.MergeAttribute("aria-label", "close");

            var spanTag = new TagBuilder("span");
            spanTag.MergeAttribute("aria-hidden", "true");
            spanTag.InnerHtml = "&times;";
            spanTag.ToString(TagRenderMode.EndTag);

            buttonTag.InnerHtml = spanTag.ToString();

            pTag.InnerHtml = buttonTag.ToString() + message;

            var htmlString = new MvcHtmlString(pTag.ToString());

            return htmlString;
        }
    }
}