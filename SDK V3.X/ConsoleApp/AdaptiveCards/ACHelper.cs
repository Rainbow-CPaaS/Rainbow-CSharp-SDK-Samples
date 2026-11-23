using Rainbow.SimpleJSON;
using System.Text;

namespace AdaptiveCards
{
    public class ACHelper
    {
        public static JSONNode? CreateRoot()
        {
            var json = Helper.GetContentOfEmbeddedResource("AC_Main.json", Encoding.UTF8);
            if (String.IsNullOrEmpty(json))
                return null;

            return JSON.Parse(json);
        }

        public static JSONNode? CreateColumn(String tittle, List<(String text, String color)> elements)
        {
            var json = Helper.GetContentOfEmbeddedResource("AC_Column.json", Encoding.UTF8);
            if (String.IsNullOrEmpty(json))
                return null;


            var jsonNode = JSON.Parse(json);
            var jItems = jsonNode?["items"];
            if ((jItems is null) || (jItems is JSONNull)) return null;

            // Set text / title
            jItems[0]["text"] = tittle;

            // Loop on each elements
            foreach (var element in elements)
            {
                JSONObject item = new();
                item["type"] = "TextBlock";
                item["text"] = element.text;
                if (!String.IsNullOrEmpty(element.color))
                    item["color"] = element.color;

                jItems.Add(item);
            }

            return jsonNode;
        }

        public static JSONNode? CreateColumnSet(List<JSONNode>? columns = null)
        {
            var json = Helper.GetContentOfEmbeddedResource("AC_ColumnSet.json", Encoding.UTF8);
            if (String.IsNullOrEmpty(json))
                return null;


            var jsonNode = JSON.Parse(json);
            var jColumns = jsonNode?["columns"];
            if ((jColumns is null) || (jColumns is JSONNull)) return null;

            // Loop on each columns
            if (columns?.Count > 0)
            {
                foreach (var column in columns)
                {
                    if ((column is null) || (column is JSONNull)) continue;
                    jColumns.Add(column);
                }
            }
            return jsonNode;
        }

        public static void AddColumnToColumnSet(JSONNode columnSet, JSONNode? column)
        {
            if ((column is null) || (column is JSONNull)) return;

            var jColumns = columnSet?["columns"];
            if ((jColumns is null) || (jColumns is JSONNull)) return;

            jColumns.Add(column);
        }

        public static Boolean AddItemToRoot(JSONNode root, JSONNode? item)
        {
            if ((item is null) || (item is JSONNull)) return false;

            var jBody = root?["body"];
            if ((jBody is null) || (jBody is JSONNull)) return false;

            jBody.Add(item);

            return true;
        }

        public static JSONNode? CreateTitle(String? title)
        {
            var json = Helper.GetContentOfEmbeddedResource("AC_Title.json", Encoding.UTF8);
            if (String.IsNullOrEmpty(json))
                return null;

            var jsonNode = JSON.Parse(json);
            if ((jsonNode is null) || (jsonNode is JSONNull)) return null;

            jsonNode["text"] = title ?? "";

            return jsonNode;
        }

        public static JSONNode? CreateSubTitle(String subTitle, String color)
        {
            var json = Helper.GetContentOfEmbeddedResource("AC_SubTitle.json", Encoding.UTF8);
            if (String.IsNullOrEmpty(json))
                return null;

            var jsonNode = JSON.Parse(json);
            if ((jsonNode is null) || (jsonNode is JSONNull)) return null;

            jsonNode["text"] = subTitle;
            jsonNode["color"] = String.IsNullOrEmpty(color) ? null : color;

            return jsonNode;
        }


        public static JSONNode? CreateCallBackItem(String label, String btnLabel, String btnId)
        {
            var json = Helper.GetContentOfEmbeddedResource("AC_CallBack.json", Encoding.UTF8);
            if (String.IsNullOrEmpty(json))
                return null;

            var jsonNode = JSON.Parse(json);
            var jItems = jsonNode?["items"];
            if ((jItems is null) || (jItems is JSONNull)) return null;

            jItems[0]["text"] = label;
            jItems[1]["actions"][0]["title"] = btnLabel;
            jItems[1]["actions"][0]["data"]["id"] = btnId;

            return jsonNode;
        }
    }
}