using AztecTariff.Models;
using Blazorise;
using System.Reflection.Metadata;
using Telerik.Documents.Core.Fonts;
using Telerik.SvgIcons;
using Telerik.Windows.Documents.Flow.FormatProviders.Html;
using Telerik.Windows.Documents.Flow.Model;
using Telerik.Windows.Documents.Flow.Model.Editing;
using Telerik.Windows.Documents.Flow.Model.Styles;
using static System.Net.Mime.MediaTypeNames;
using DocXToPdfConverter;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using FontSize = DocumentFormat.OpenXml.Wordprocessing.FontSize;
using Underline = DocumentFormat.OpenXml.Wordprocessing.Underline;
using ParagraphProperties = DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties;
using TableCell = DocumentFormat.OpenXml.Wordprocessing.TableCell;
using TableCellProperties = DocumentFormat.OpenXml.Wordprocessing.TableCellProperties;
using Document = DocumentFormat.OpenXml.Wordprocessing.Document;
using TableProperties = DocumentFormat.OpenXml.Wordprocessing.TableProperties;
using TableBorders = DocumentFormat.OpenXml.Wordprocessing.TableBorders;
using Table = DocumentFormat.OpenXml.Wordprocessing.Table;
using TableRow = DocumentFormat.OpenXml.Wordprocessing.TableRow;
using OpenXmlPowerTools;
using Footer = DocumentFormat.OpenXml.Wordprocessing.Footer;
using SectionProperties = DocumentFormat.OpenXml.Wordprocessing.SectionProperties;
using Italic = DocumentFormat.OpenXml.Wordprocessing.Italic;
using DocumentFormat.OpenXml.Drawing.Charts;
using File = System.IO.File;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using Bold = DocumentFormat.OpenXml.Wordprocessing.Bold;

namespace AztecTariff.Services
{
    public class PDFMakerService
    {
        bool includeABV;
        int ColumnCount = 2;
        string template;
        // initialize LibreOffice soffice.exe filepath
        string locationOfLibreOfficeSoffice = @"C:\Users\AdamM2\Downloads\LibreOfficePortable\LibreOfficePortable.exe";
        //string outputPath = @"C:\Users\AdamM2\OneDrive - Zonal Retail Data Systems Limited\Desktop\TariffNotesNStuff\Test.docx";
        Models.Settings _settings;
        public async Task<string> MakePdf(FullSalesArea pdfDocMod, string _template, bool _includeABV)
        {
            string outputPath = Path.Combine(_settings.CSVFileLocation, "Test.docx");
            includeABV = _includeABV;
            template = _template;  
            string docxPath = "";
            string fileName = $"{DateTime.Now.ToString("mmhhddMMyyyy")}{pdfDocMod.TariffName}.pdf";

            switch (template)
            {
                case "Single-Page":
                    var totalLinesRequired = pdfDocMod.Categories.Sum(c => c.LinesRequired);
                    docxPath = GenerateSinglePageTariff(pdfDocMod, outputPath);
                    break;
                case "Multi-Page":
                    ColumnCount = 1;
                    docxPath = GenerateMultiPageTariff(pdfDocMod, outputPath, false);
                    break;
                case "Landscape Multi-Page":
                    ColumnCount = 1;
                    docxPath = GenerateMultiPageTariff(pdfDocMod, outputPath, true);
                    break;
                default:
                    break;
            }

            return ConvertDocxToPDF(docxPath, fileName);
        }

        public PDFMakerService(Models.Settings settings)
        {
            _settings = settings;
        }

        private string ConvertDocxToPDF(string docxPath, string filename)
        {
            var reportgen = new ReportGenerator(_settings.LibreLocation);
            var outputpath = Path.Combine(_settings.CSVFileLocation, filename);
            reportgen.Convert(docxPath, outputpath);
            return outputpath;


        }

        public string GenerateSinglePageTariff(FullSalesArea fullSalesArea, string outputPath, bool rerun = false)
        {
            int totalLinesRequired = fullSalesArea.Categories.Sum(p => p.LinesRequired);

            List<List<FullCategory>> columns;
            string templateLocation = "";

            if (totalLinesRequired < 70)
            {
                templateLocation = Path.Combine(_settings.TemplateFolderLocation, "TEMPLATE2.docx");
                ColumnCount = 2;
                columns = SplitByColumns(fullSalesArea.Categories, 2, 48);
            }
            else if (totalLinesRequired < 100)
            {
                templateLocation = Path.Combine(_settings.TemplateFolderLocation, "TEMPLATE3.docx");
                ColumnCount = 3;
                columns = SplitByColumns(fullSalesArea.Categories, 3, 48);
            }
            else
            {
                templateLocation = Path.Combine(_settings.TemplateFolderLocation, "TEMPLATE4.docx");
                ColumnCount = 4;
                columns = SplitByColumns(fullSalesArea.Categories, 4, 48);
            } 


            File.Delete(outputPath);
            File.Copy(templateLocation, outputPath);
            using (WordprocessingDocument doc = WordprocessingDocument.Open(outputPath, true))
            {
                #region Header
                foreach (var headerPart in doc.MainDocumentPart.HeaderParts)
                {
                    //Gets the text in headers
                    var y = headerPart.RootElement.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>();
                    foreach (var currentText in y)
                    {
                        currentText.Text = currentText.Text.Replace("[TITLE]", fullSalesArea.TariffName);
                        currentText.Text = currentText.Text.Replace("[SUBTITLE]", fullSalesArea.HeaderMessage);
                    }
                    

                    
                }             
                
                #endregion
                #region Tables

                Table table = doc.MainDocumentPart.Document.Body.Elements<Table>().First();
                TableRow row = table.Elements<TableRow>().ElementAt(0);

                int columnIterator = 0;
                foreach (var columnCell in row.Elements<TableCell>())
                {
                    Table columnTable = columnCell.Elements<Table>().First();
                    int i = 0;
                    foreach (var category in columns[columnIterator])
                    {
                        if (!category.IncludedInPDFProducts.Any()) continue;
                        
                        var categoryRow = columnTable.Elements<TableRow>().ElementAt(i);
                        var categoryCell1 = categoryRow.Elements<TableCell>().ElementAt(0);
                        var categoryCell2 = categoryRow.Elements<TableCell>().ElementAt(1);
                        var categoryCell3 = categoryRow.Elements<TableCell>().ElementAt(2);

                        categoryCell1.RemoveAllChildren();

                        var cell1Props = new TableCellProperties();
                        cell1Props.Append(new HorizontalMerge()
                        {
                            Val = MergedCellValues.Restart
                        });
                        var cell2Props = new TableCellProperties();
                        cell2Props.Append(new HorizontalMerge()
                        {
                            Val = MergedCellValues.Continue
                        });
                        var cell3Props = new TableCellProperties();
                        cell3Props.Append(new HorizontalMerge()
                        {
                            Val = MergedCellValues.Continue
                        });

                        categoryCell1.Append(cell1Props);
                        categoryCell2.Append(cell2Props);
                        categoryCell3.Append(cell3Props);


                        categoryCell1.Append(CategoryText(category.TariffCategory));
                        i++;

                        if (category.SummarizedCategory != null)
                        {
                            var tableRow = columnTable.Elements<TableRow>().ElementAt(i);
                            var firstCell = tableRow.Elements<TableCell>().ElementAt(0);
                            var secondCell = tableRow.Elements<TableCell>().ElementAt(1);
                            var thirdCell = tableRow.Elements<TableCell>().ElementAt(2);


                            firstCell.RemoveAllChildren();
                            secondCell.RemoveAllChildren();
                            thirdCell.RemoveAllChildren();

                            firstCell.Append(StandardText($"{category.SummarizedCategory.SummaryDescription} £{category.SummarizedCategory.MinPrice} - £{category.SummarizedCategory.MaxPrice}"));
                            i++;
                        }
                        else
                        {
                            foreach (var product in category.IncludedInPDFProducts)
                            {
                                try
                                {
                                    var tableRow = columnTable.Elements<TableRow>().ElementAt(i);
                                    var firstCell = tableRow.Elements<TableCell>().ElementAt(0);
                                    var secondCell = tableRow.Elements<TableCell>().ElementAt(1);
                                    var thirdCell = tableRow.Elements<TableCell>().ElementAt(2);


                                    firstCell.RemoveAllChildren();
                                    secondCell.RemoveAllChildren();
                                    thirdCell.RemoveAllChildren();

                                    firstCell.Append(StandardText(product.ProductTariffName));
                                    if (product.ABV == 0 || !includeABV)
                                    {
                                        secondCell.Append(StandardText($""));
                                    }
                                    else
                                    {
                                        secondCell.Append(ItalicText($"{product.ABV.ToString()}%"));
                                    }

                                    thirdCell.Append(StandardText($"£{product.Price.ToString("0.00")}"));
                                    i++;
                                }
                                catch
                                {
                                    break;
                                }
                            }
                        }
                    }

                    List<TableRow> rowsToDelete = new List<TableRow>();
                    foreach (var tablerow in columnTable.Elements<TableRow>())
                    {
                        if (string.IsNullOrWhiteSpace(tablerow.InnerText))
                        {
                            rowsToDelete.Add(tablerow);
                        }
                    }

                    foreach (var r in rowsToDelete)
                    {
                        columnTable.RemoveChild(r);
                    }
                    columnIterator++;
                }
                #endregion
                #region Footer
                foreach (var footerPart in doc.MainDocumentPart.FooterParts)
                {
                    //Gets the text in headers
                    foreach (var currentText in footerPart.RootElement.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>())
                    {
                        currentText.Text = currentText.Text.Replace("[FOOTER]", fullSalesArea.FooterMessage);
                    }
                }
                #endregion
            }



            return outputPath;
        }

        private List<int> GetTrueColumnLengths(List<List<FullCategory>> columns)
        {

            List<int> columnLengths = new List<int>();
            foreach (var column in columns)
            {

                var allProds = column.SelectMany(c => c.IncludedInPDFProducts);
                int numberOfTwoLiners = allProds.Where(p => p.ProductTariffName.Length > 15).Count();

                int columnLength = allProds.Count() + numberOfTwoLiners + column.Count;
                columnLengths.Add(columnLength);
            }

            return columnLengths;
        }
        private int GetTrueColumnLength(List<FullCategory> column)
        {
            var allProds = column.SelectMany(c => c.IncludedInPDFProducts);
            int numberOfTwoLiners = allProds.Where(p => p.ProductTariffName.Length > 15).Count();

            return allProds.Count() + numberOfTwoLiners + column.Count;
        }


        public string GenerateMultiPageTariff(FullSalesArea fullSalesArea, string outputPath, bool landscape)
        {
            string templateLocation;
            if (landscape)
            {
                templateLocation = Path.Combine(_settings.TemplateFolderLocation, "TEMPLATE1L.docx");
            }
            else
            {
                templateLocation = Path.Combine(_settings.TemplateFolderLocation, "TEMPLATE1M.docx");
            }

            File.Delete(outputPath);
            File.Copy(templateLocation, outputPath);
            var columns = SplitByColumns(fullSalesArea.Categories, 1, 5000);
            using (WordprocessingDocument doc = WordprocessingDocument.Open(outputPath, true))
            {
                #region Header
                foreach (var headerPart in doc.MainDocumentPart.HeaderParts)
                {
                    //Gets the text in headers
                    foreach (var currentText in headerPart.RootElement.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>())
                    {
                        currentText.Text = currentText.Text.Replace("[TITLE]", fullSalesArea.TariffName);
                        currentText.Text = currentText.Text.Replace("[SUBTITLE]", fullSalesArea.HeaderMessage);
                    }
                }
                #endregion
                #region Tables

                Table table = doc.MainDocumentPart.Document.Body.Elements<Table>().First();
                TableRow row = table.Elements<TableRow>().ElementAt(0);

                int columnIterator = 0;
                foreach (var columnCell in row.Elements<TableCell>())
                {
                    Table columnTable = columnCell.Elements<Table>().First();
                    int i = 0;
                    foreach (var category in columns[columnIterator])
                    {
                        if (!category.IncludedInPDFProducts.Any()) continue;
                        var categoryRow = columnTable.Elements<TableRow>().ElementAt(i);
                        var categoryCell1 = categoryRow.Elements<TableCell>().ElementAt(0);
                        var categoryCell2 = categoryRow.Elements<TableCell>().ElementAt(1);
                        var categoryCell3 = categoryRow.Elements<TableCell>().ElementAt(2);

                        categoryCell1.RemoveAllChildren();

                        var cell1Props = new TableCellProperties();
                        cell1Props.Append(new HorizontalMerge()
                        {
                            Val = MergedCellValues.Restart
                        });
                        var cell2Props = new TableCellProperties();
                        cell2Props.Append(new HorizontalMerge()
                        {
                            Val = MergedCellValues.Continue
                        });
                        var cell3Props = new TableCellProperties();
                        cell3Props.Append(new HorizontalMerge()
                        {
                            Val = MergedCellValues.Continue
                        });

                        categoryCell1.Append(cell1Props);
                        categoryCell2.Append(cell2Props);
                        categoryCell3.Append(cell3Props);


                        categoryCell1.Append(CategoryText(category.TariffCategory));
                        i++;

                        if (category.SummarizedCategory != null)
                        {
                            var tableRow = columnTable.Elements<TableRow>().ElementAt(i);
                            var firstCell = tableRow.Elements<TableCell>().ElementAt(0);
                            var secondCell = tableRow.Elements<TableCell>().ElementAt(1);
                            var thirdCell = tableRow.Elements<TableCell>().ElementAt(2);


                            firstCell.RemoveAllChildren();
                            secondCell.RemoveAllChildren();
                            thirdCell.RemoveAllChildren();

                            firstCell.Append(StandardText($"{category.SummarizedCategory.SummaryDescription} £{category.SummarizedCategory.MinPrice} - £{category.SummarizedCategory.MaxPrice}"));
                        }
                        else
                        {
                            foreach (var product in category.IncludedInPDFProducts)
                            {
                                try
                                {
                                    var tableRow = columnTable.Elements<TableRow>().ElementAt(i);
                                    var firstCell = tableRow.Elements<TableCell>().ElementAt(0);
                                    var secondCell = tableRow.Elements<TableCell>().ElementAt(1);
                                    var thirdCell = tableRow.Elements<TableCell>().ElementAt(2);


                                    firstCell.RemoveAllChildren();
                                    secondCell.RemoveAllChildren();
                                    thirdCell.RemoveAllChildren();

                                    firstCell.Append(StandardText(product.ProductTariffName));
                                    if (product.ABV == 0 || !includeABV)
                                    {
                                        secondCell.Append(StandardText($""));
                                    }
                                    else
                                    {
                                        secondCell.Append(ItalicText($"{product.ABV.ToString()}%"));
                                    }

                                    thirdCell.Append(StandardText($"£{product.Price.ToString("0.00")}"));
                                    i++;
                                }
                                catch
                                {
                                    break;
                                }
                            }
                        }
                    }

                    List<TableRow> rowsToDelete = new List<TableRow>();
                    foreach (var tablerow in columnTable.Elements<TableRow>())
                    {
                        if (string.IsNullOrWhiteSpace(tablerow.InnerText))
                        {
                            rowsToDelete.Add(tablerow);
                        }
                    }

                    foreach (var r in rowsToDelete)
                    {
                        columnTable.RemoveChild(r);
                    }
                    columnIterator++;
                }
                #endregion
                #region Footer
                foreach (var footerPart in doc.MainDocumentPart.FooterParts)
                {
                    //Gets the text in headers
                    foreach (var currentText in footerPart.RootElement.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>())
                    {
                        currentText.Text = currentText.Text.Replace("[FOOTER]", fullSalesArea.FooterMessage);
                    }
                }
                #endregion
            }

            return outputPath;
        }

        private OpenXmlElement CategoryText(string v)
        {
            var ul = new Underline() { Val = DocumentFormat.OpenXml.Wordprocessing.UnderlineValues.Single };
            int fontSize = 12;
            switch (ColumnCount)
            {
                case 1:
                    fontSize = 35;
                    break;
                case 2:
                    fontSize = 30;
                    break;
                case 3:
                    fontSize = 25;
                    break;
                case 4:
                    fontSize = 20;
                    break;
            }
            FontSize fs = new FontSize() { Val = fontSize.ToString() };

            return new Paragraph(new Run(new RunProperties(fs, ul), new Text(v)));
        }

        private OpenXmlElement StandardText(string v)
        {
            var unbold = new Bold() { Val = false };
            int fontSize = 12;
            switch (ColumnCount)
            {
                case 1:
                    fontSize = 33;
                    break;
                case 2:
                    fontSize = 23;
                    break;
                case 3:
                    fontSize = 17;
                    break;
                case 4:
                    fontSize = 16;
                    break;
            }
            FontSize fs = new FontSize() { Val = fontSize.ToString() };

            return new Paragraph(new Run(new RunProperties(fs, unbold), new Text(v)));
        }

        private OpenXmlElement ItalicText(string v)
        {
            var unbold = new Bold() { Val = false };
            var italic = new Italic() { Val = true };
            int fontSize = 12;
            switch (ColumnCount)
            {
                case 1:
                    fontSize = 24;
                    break;
                case 2:
                    fontSize = 22;
                    break;
                case 3:
                    fontSize = 17;
                    break;
                case 4:
                    fontSize = 13;
                    break;
            }
            FontSize fs = new FontSize() { Val = fontSize.ToString() };

            return new Paragraph(new Run(new RunProperties(italic, fs, unbold), new Text(v)));
        }




        public List<List<FullCategory>> SplitByColumns(List<FullCategory> list, int numberOfColumns, int maxRowsPerColumn)
        {
            List<List<FullCategory>> result = new List<List<FullCategory>>();
            int totalLines = list.Sum(x => x.LinesRequired);
            int averageLinesPerColumn = totalLines / numberOfColumns;
            int categoryCount = 0;

            for (int i = 0; i < numberOfColumns; i++)
            {
                int lineCount = 0;
                var colcats = new List<FullCategory>();

                while (lineCount < averageLinesPerColumn && GetTrueColumnLength(colcats) < maxRowsPerColumn)
                {
                    try
                    {
                        colcats.Add(list[categoryCount]);
                        lineCount += list[categoryCount].LinesRequired;
                        categoryCount++;
                        if (categoryCount >= list.Count) break;
                    }
                    catch
                    {
                        break;
                    }
                }

                result.Add(colcats);
            }

            //If there are remaining items, distribute them among columns
            if (template == "Multi-Page")
            {
                return result;
            }
            while (categoryCount < list.Count)
            {
                int columnIndex = 0;
                while (categoryCount < list.Count && GetTrueColumnLength(result[columnIndex]) < maxRowsPerColumn)
                {
                    result[columnIndex].Add(list[categoryCount]);
                    categoryCount++;
                    columnIndex++;
                    if (columnIndex >= numberOfColumns) columnIndex = 0;
                }
            }

            return result;
        }
    }
}
