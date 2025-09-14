using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace Utility
{
    internal class XmlLoader
    {
        XmlDocument doc;
        string xmlPath;
        string xsltPath;
        string xmlParseError;

        public XmlLoader()
        {

        }

        public void Load(string path)
        {
            try
            {
                xmlPath = path;
                // base dir
                var baseDir = System.IO.Path.GetDirectoryName(path);
                // XMLファイルを読み込む
                doc = new XmlDocument();
                doc.Load(path);

                // ドキュメント内の全ノードを走査する
                foreach (XmlNode node in doc.ChildNodes)
                {
                    // ノードがProcessingInstructionであるかを確認
                    if (node.NodeType == XmlNodeType.ProcessingInstruction)
                    {
                        var pi = (XmlProcessingInstruction)node;
                        // ターゲット名が「xml-stylesheet」であるかを確認
                        if (pi.Name == "xml-stylesheet")
                        {
                            // データ属性をパースしてhrefの値を取得
                            string hrefValue = GetAttributeFromProcessingInstruction(pi.Data, "href");
                            // stylesheetファイル存在チェック
                            xsltPath = System.IO.Path.Combine(baseDir, hrefValue);
                            if (System.IO.File.Exists(xsltPath))
                            {
                                //// XSLTファイルを読み込む
                                //var xsl = new System.Xml.Xsl.XslCompiledTransform();
                                //xsl.Load(xsltPath);
                                //// 変換結果を新しいXmlDocumentに保存
                                //var transformedDoc = new XmlDocument();
                                //using (var writer = transformedDoc.CreateNavigator().AppendChild())
                                //{
                                //    xsl.Transform(doc, writer);
                                //}
                                //// 元のドキュメントを変換後のドキュメントに置き換え
                                //doc = transformedDoc;
                            }
                            else
                            {
                                xsltPath = null;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                xmlParseError = ex.Message;
            }
        }

        private static string GetAttributeFromProcessingInstruction(string data, string attributeName)
        {
            var parts = data.Split(' ');
            foreach (var part in parts)
            {
                if (part.StartsWith(attributeName + "="))
                {
                    // 属性値から引用符を削除して返す
                    return part.Substring(attributeName.Length + 2, part.Length - attributeName.Length - 3);
                }
            }
            return null;
        }

        public string GetView()
        {
            try
            {
                if (!(xmlParseError is null))
                {
                    throw new Exception(xmlParseError);
                }

                if (xsltPath is null)
                {
                    if (doc is null)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return doc.ToString();
                    }
                }
                else
                {
                    XmlReaderSettings settings = new XmlReaderSettings
                    {
                        DtdProcessing = DtdProcessing.Parse,
                    };

                    using (XmlReader reader = XmlReader.Create(xmlPath, settings))
                    {
                        // XSLTの適用
                        XslCompiledTransform xslt = new XslCompiledTransform();
                        XsltSettings xsltSettings = new XsltSettings
                        {
                            EnableDocumentFunction = true,
                            //EnableScript = true
                        };
                        // XSLTスタイルシートをロード
                        xslt.Load(xsltPath, xsltSettings, new XmlUrlResolver());

                        // HTMLとして出力
                        using (StringWriter writer = new StringWriter())
                        {
                            xslt.Transform(reader, null, writer);
                            return writer.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //return ex.Message;
                return $@"
<html>
<head><meta charset='UTF-8'></head>
<body>
<h1>XMLの変換に失敗しました。</h1>
<p>XMLまたはXSLTの内容を確認してください。</p>
<h2>Error</h2>
<p>
${ex.Message}
</p>
</body>
";
            }
            //return string.Empty;
        }
    }
}
