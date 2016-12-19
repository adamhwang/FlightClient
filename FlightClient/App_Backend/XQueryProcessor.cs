using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using System.Xml;

using Saxon.Api;

namespace FlightClient
{

    public class xQueryProcessor
    {
        private Processor processor;
        private XQueryCompiler compiler;
        private XQueryEvaluator xqueryEvaluator;

        public xQueryProcessor()
        {
            processor = new Processor();
            compiler = processor.NewXQueryCompiler();
        }

        public void Load(string Query)
        {
            xqueryEvaluator = compiler.Compile(Query).Load();
        }

        public void LoadFromFile(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                xqueryEvaluator = compiler.Compile(fs).Load();
            }
        }

        public XmlNode RunQuery(XmlNode xmlNode)
        {
            XdmNode indoc = processor.NewDocumentBuilder().Build(new XmlNodeReader(xmlNode));
            DomDestination dest = new DomDestination();
            xqueryEvaluator.ContextItem = indoc;
            xqueryEvaluator.Run(dest);
            return dest.XmlDocument;
        }

        public static XmlNode RunQuery(XmlNode xmlNode, string XQuery)
        {
            xQueryProcessor saxonXQuery = new xQueryProcessor();
            saxonXQuery.Load(XQuery);
            return saxonXQuery.RunQuery(xmlNode);
        }

        //public void RunQuery(XmlNode xmlNode, XmlWriter output)
        //{
        //    XdmNode indoc = processor.NewDocumentBuilder().Build(new XmlNodeReader(xmlNode));
        //    TextWriterDestination twd = new TextWriterDestination(output);
        //    xqueryEvaluator.ContextItem = indoc;
        //    xqueryEvaluator.Run(twd);
        //    twd.Close();
        //}



        //public static void RunQuery(XmlNode inputNode, string XQuery, XmlWriter outputWriter)
        //{
        //    xQueryProcessor saxonXQuery = new xQueryProcessor();
        //    saxonXQuery.Load(XQuery);
        //    saxonXQuery.RunQuery(inputNode, outputWriter);
        //}
    }

}