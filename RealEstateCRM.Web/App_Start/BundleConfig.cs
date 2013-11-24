using System.Web;
using System.Web.Optimization;

namespace RealEstateCRM.Web
{
    public class BundleConfig
    {
        // 有关 Bundling 的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                "~/Scripts/jquery.ui.datepicker-zh-CN.js",
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include( 
                "~/Scripts/jquery.validate.min.js",
                "~/Scripts/jquery.validate.unobtrusive.min.js"
                       ));


            // 使用 Modernizr 的开发版本进行开发和了解信息。然后，当你做好
            // 生产准备时，请使用 http://modernizr.com 上的生成工具来仅选择所需的测试。
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-{version}.min.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/Site.css"));

            bundles.Add(new StyleBundle("~/Content/redmond").Include(
                        "~/Content/redmond/jquery-ui-{version}.custom.css"));

            //bundles.Add(new ScriptBundle("~/bundles/BootstrapJs").Include(
            //            "~/bootstrap/js/bootstrap.js"));

            //bundles.Add(new ScriptBundle("~/bundles/BootstrapCss").Include(
            //            "~/bootstrap/css/bootstrap.css"));

            bundles.Add(new ScriptBundle("~/bundles/BootstrapCss").Include(
                        "~/bootstrap/css/bootstrap.css"));
            bundles.Add(new ScriptBundle("~/bundles/BootstrapJs").Include(
                       "~/scripts/bootstrap.js"));
            bundles.Add(new ScriptBundle("~/bundles/MyJs").Include(
                        "~/Scripts/MyJs.js"));

            //bundles.Add(new StyleBundle("~/bundles/jqgridcss").Include(
            //    "~/scripts/jqgrid4/css/ui.jqgrid.css",
            //    "~/scripts/jqgrid4/css/ui.jqgrid.css",
            //    "~/scripts/jqgrid4/plugins/ui.multiselect.css"
            //    ));
            //bundles.Add(new ScriptBundle("~/bundles/jqgrid").Include(
            //    "~/scripts/jqgrid4/js/grid.locale-cn.js",
            //    "~/scripts/jqgrid4/js/grid.locale-cn.js",
            //    "~/scripts/jqgrid4/plugins/ui.multiselect.js",
            //    "~/scripts/jqgrid4/js/jquery.jqGrid.src.js",
            //    "~/scripts/jqgrid4/plugins/grid.postext.js"));
            bundles.Add(new ScriptBundle("~/bundles/jqGrid").Include(
                        "~/Scripts/i18n/grid.locale-cn.js",
                        "~/Scripts/JqGrid4/ui.multiselect.js",
                        "~/Scripts/jquery.jqGrid.js",
                        "~/Scripts/JqGrid4/grid.postext.js"));
            // css
            bundles.Add(new StyleBundle("~/Content/jqGrid/css").Include(
                                "~/Content/jquery.jqGrid/ui.jqgrid.css",
                                 "~/Scripts/JqGrid4/ui.multiselect.css"
                                ));

            bundles.Add(new ScriptBundle("~/Content/zTree").Include(
                "~/Content/zTree/js/jquery.ztree.all-3.5.js"));

            // css
            bundles.Add(new StyleBundle("~/Content/zTree/css").Include(
                                "~/Content/zTree/css/zTreeStyle/zTreeStyle.css"));
            bundles.Add(new ScriptBundle("~/bundles/backbone").Include(
              "~/scripts/underscore-min.js",
              "~/scripts/backbone-min.js"));
        }
    }
}