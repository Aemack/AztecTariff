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
        RadFlowDocument document;
        RadFlowDocumentEditor editor;
        List<Product> IncludedProducts;

        int ColumnCount = 2;
        float cellWidth => 9800 / ColumnCount;
        int fontSize = 22;
        int rowsPerColumn = 45;
        string template;
        // initialize LibreOffice soffice.exe filepath
        string locationOfLibreOfficeSoffice = @"C:\Users\AdamM2\Downloads\LibreOfficePortable\LibreOfficePortable.exe";
        string outputPath = @"C:\Users\AdamM2\OneDrive - Zonal Retail Data Systems Limited\Desktop\TariffNotesNStuff\Test.docx";
        public async Task<string> MakePdf(FullSalesArea pdfDocMod, string _template)
        {
            template = _template;
            string docxPath = "";
            switch (template)
            {
                case "Single-Page":
                    var totalLinesRequired = pdfDocMod.Categories.Sum(c => c.LinesRequired);
                    docxPath = GenerateSinglePageTariff(pdfDocMod, outputPath);
                    break;
                case "Multi-Page":
                    docxPath = GenerateMultiPageTariff(pdfDocMod, outputPath);
                    break;
                default:
                    break;
            }

            return ConvertDocxToPDF(docxPath);
        }


        private string ConvertDocxToPDF(string docxPath)
        {
            var test = new ReportGenerator(locationOfLibreOfficeSoffice);
            // convert DOCX to PDF
            test.Convert(docxPath, @$"Data\tmpPdf.pdf");
            return @$"Data\tmpPdf.pdf";
        }

        public string GenerateTwoColumnTariff(FullSalesArea fullSalesArea, string outputPath)
        {
            var templateLocation = @"C:\Users\AdamM2\OneDrive - Zonal Retail Data Systems Limited\Desktop\TariffNotesNStuff\TEMPLATE2.docx";
            File.Delete(outputPath);
            File.Copy(templateLocation, outputPath);
            var columns = SplitByColumns(fullSalesArea.Categories, 2);
            int colTotal = columns[0].Sum(x => x.LinesRequired);
            int col2Total = columns[1].Sum(x => x.LinesRequired);
            using (WordprocessingDocument doc = WordprocessingDocument.Open(outputPath, true))
            {
                #region Header
                foreach (var headerPart in doc.MainDocumentPart.HeaderParts)
                {
                    //Gets the text in headers
                    foreach (var currentText in headerPart.RootElement.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>())
                    {
                        currentText.Text = currentText.Text.Replace("[TITLE]", fullSalesArea.TariffName);
                    }
                }
                #endregion
                #region Tables
                
                Table table = doc.MainDocumentPart.Document.Body.Elements<Table>().First();
                TableRow row = table.Elements<TableRow>().ElementAt(0);

                #region First Column
                TableCell firstColumnCell = row.Elements<TableCell>().ElementAt(0);
                Table firstColumnTable = firstColumnCell.Elements<Table>().First();
                int i = 0;
                foreach (var category in columns[0])
                {
                    var categoryRow = firstColumnTable.Elements<TableRow>().ElementAt(i);
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
                    foreach (var product in category.IncludedProducts)
                    {
                        try
                        {
                            var tableRow = firstColumnTable.Elements<TableRow>().ElementAt(i);
                            var firstCell = tableRow.Elements<TableCell>().ElementAt(0);
                            var secondCell = tableRow.Elements<TableCell>().ElementAt(1);
                            var thirdCell = tableRow.Elements<TableCell>().ElementAt(2);


                            firstCell.RemoveAllChildren();
                            secondCell.RemoveAllChildren();
                            thirdCell.RemoveAllChildren();

                            firstCell.Append(StandardText(product.ProductTariffName));
                            if (product.ABV == 0)
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

                List<TableRow> rowsToDelete = new List<TableRow>();
                foreach (var tablerow in firstColumnTable.Elements<TableRow>())
                {

                    //if (tablerow.Descendants<Text>().Select(x => string.IsNullOrWhiteSpace(x.Text)).Count() >= 3)
                    if (string.IsNullOrWhiteSpace(tablerow.InnerText))
                    {
                        rowsToDelete.Add(tablerow);
                    }
                }

                foreach (var r in rowsToDelete)
                {
                    firstColumnTable.RemoveChild(r);
                }
                #endregion
                #region Second Column
                TableCell secondColumnCell = row.Elements<TableCell>().ElementAt(1);
                Table secondColumnTable = secondColumnCell.Elements<Table>().First();
                i = 0;
                foreach (var category in columns[1])
                {
                    var categoryRow = secondColumnTable.Elements<TableRow>().ElementAt(i);
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
                    foreach (var product in category.IncludedProducts)
                    {
                        try
                        {
                            var tableRow = secondColumnTable.Elements<TableRow>().ElementAt(i);
                            var firstCell = tableRow.Elements<TableCell>().ElementAt(0);
                            var secondCell = tableRow.Elements<TableCell>().ElementAt(1);
                            var thirdCell = tableRow.Elements<TableCell>().ElementAt(2);

                            firstCell.RemoveAllChildren();
                            secondCell.RemoveAllChildren();
                            thirdCell.RemoveAllChildren();

                            firstCell.Append(StandardText(product.ProductTariffName));
                            if (product.ABV == 0)
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

                rowsToDelete = new List<TableRow>();
                foreach (var tablerow in secondColumnTable.Elements<TableRow>())
                {
                    if (string.IsNullOrWhiteSpace(tablerow.InnerText))
                    {
                        rowsToDelete.Add(tablerow);
                    }
                }

                foreach (var r in rowsToDelete)
                {
                    secondColumnTable.RemoveChild(r);
                }
                #endregion

                #region First Column

                //TableCell firstColumnCell = row.Elements<TableCell>().ElementAt(0);
                //Table firstColumnTable = firstColumnCell.Elements<Table>().First();
                //int i = 0;
                //foreach (var category in columns[0])
                //{
                //    var categoryRow = firstColumnTable.Elements<TableRow>().ElementAt(i);
                //    var categoryCell = categoryRow.Elements<TableCell>().ElementAt(0);
                //    categoryCell.RemoveAllChildren();
                //    categoryCell.Append(CategoryText($"{category.TariffCategory}"));
                //    i++;
                //    foreach (var product in category.IncludedProducts)
                //    {
                //        try
                //        {
                //            var tableRow = firstColumnTable.Elements<TableRow>().ElementAt(i);
                //            var firstCell = tableRow.Elements<TableCell>().ElementAt(0);
                //            var secondCell = tableRow.Elements<TableCell>().ElementAt(1);
                //            var thirdCell = tableRow.Elements<TableCell>().ElementAt(2);


                //            firstCell.RemoveAllChildren();
                //            secondCell.RemoveAllChildren();
                //            thirdCell.RemoveAllChildren();

                //            firstCell.Append(StandardText(product.ProductTariffName));
                //            if (product.ABV == 0)
                //            {
                //                secondCell.Append(StandardText($""));
                //            }
                //            else
                //            {
                //                secondCell.Append(StandardText($"{product.ABV.ToString()}%"));
                //            }

                //            thirdCell.Append(StandardText($"£{product.Price.ToString("0.00")}"));
                //            i++;
                //        }
                //        catch
                //        {
                //            break;
                //        }
                //    }
                //}

                //List<TableRow> rowsToDelete = new List<TableRow>();
                //foreach (var tablerow in firstColumnTable.Elements<TableRow>())
                //{

                //    //if (tablerow.Descendants<Text>().Select(x => string.IsNullOrWhiteSpace(x.Text)).Count() >= 3)
                //    if (string.IsNullOrWhiteSpace(tablerow.InnerText))
                //    {
                //        rowsToDelete.Add(tablerow);
                //    }
                //}

                //foreach (var r in rowsToDelete)
                //{
                //    firstColumnTable.RemoveChild(r);
                //}
                //#endregion
                //#region Second Column

                //TableCell secondColumnCell = row.Elements<TableCell>().ElementAt(1);
                //Table secondColumnTable = secondColumnCell.Elements<Table>().First();
                //i = 0;
                //foreach (var category in columns[1])
                //{
                //    var categoryRow = secondColumnTable.Elements<TableRow>().ElementAt(i);
                //    var categoryCell = categoryRow.Elements<TableCell>().ElementAt(0);
                //    categoryCell.RemoveAllChildren();


                //    categoryCell.Append(CategoryText(category.TariffCategory));
                //    i++;
                //    foreach (var product in category.IncludedProducts)
                //    {
                //        try
                //        {
                //            var tableRow = secondColumnTable.Elements<TableRow>().ElementAt(i);
                //            var firstCell = tableRow.Elements<TableCell>().ElementAt(0);
                //            var secondCell = tableRow.Elements<TableCell>().ElementAt(1);
                //            var thirdCell = tableRow.Elements<TableCell>().ElementAt(2);

                //            firstCell.RemoveAllChildren();
                //            secondCell.RemoveAllChildren();
                //            thirdCell.RemoveAllChildren();

                //            firstCell.Append(StandardText(product.ProductTariffName));
                //            if (product.ABV == 0)
                //            {
                //                secondCell.Append(StandardText($""));
                //            }
                //            else
                //            {
                //                secondCell.Append(StandardText($"{product.ABV.ToString()}%"));
                //            }
                //            thirdCell.Append(StandardText($"£{product.Price.ToString("0.00")}"));
                //            i++;
                //        }
                //        catch
                //        {
                //            break;
                //        }
                //    }
                //}

                //rowsToDelete = new List<TableRow>();
                //foreach (var tablerow in secondColumnTable.Elements<TableRow>())
                //{
                //    if (string.IsNullOrWhiteSpace(tablerow.InnerText))
                //    {
                //        rowsToDelete.Add(tablerow);
                //    }
                //}

                //foreach (var r in rowsToDelete)
                //{
                //    secondColumnTable.RemoveChild(r);
                //}

                #endregion
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

        public string GenerateThreeColumnTariff(FullSalesArea fullSalesArea, string outputPath)
        {
            var templateLocation = @"C:\Users\AdamM2\OneDrive - Zonal Retail Data Systems Limited\Desktop\TariffNotesNStuff\TEMPLATE3.docx";
            File.Delete(outputPath);
            File.Copy(templateLocation, outputPath);
            var columns = SplitByColumns(fullSalesArea.Categories, 3);
            int colTotal = columns[0].Sum(x => x.LinesRequired);
            int col2Total = columns[1].Sum(x => x.LinesRequired);
            using (WordprocessingDocument doc = WordprocessingDocument.Open(outputPath, true))
            {
                #region Header
                foreach (var headerPart in doc.MainDocumentPart.HeaderParts)
                {
                    //Gets the text in headers
                    foreach (var currentText in headerPart.RootElement.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>())
                    {
                        currentText.Text = currentText.Text.Replace("[TITLE]", fullSalesArea.TariffName);
                    }
                }
                #endregion
                #region Tables
                Table table = doc.MainDocumentPart.Document.Body.Elements<Table>().First();
                TableRow row = table.Elements<TableRow>().ElementAt(0);
                #region First Column
                TableCell firstColumnCell = row.Elements<TableCell>().ElementAt(0);
                Table firstColumnTable = firstColumnCell.Elements<Table>().First();
                int i = 0;
                foreach (var category in columns[0])
                {
                    var categoryRow = firstColumnTable.Elements<TableRow>().ElementAt(i);
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
                    foreach (var product in category.IncludedProducts)
                    {
                        try
                        {
                            var tableRow = firstColumnTable.Elements<TableRow>().ElementAt(i);
                            var firstCell = tableRow.Elements<TableCell>().ElementAt(0);
                            var secondCell = tableRow.Elements<TableCell>().ElementAt(1);
                            var thirdCell = tableRow.Elements<TableCell>().ElementAt(2);


                            firstCell.RemoveAllChildren();
                            secondCell.RemoveAllChildren();
                            thirdCell.RemoveAllChildren();

                            firstCell.Append(StandardText(product.ProductTariffName));
                            if (product.ABV == 0)
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

                List<TableRow> rowsToDelete = new List<TableRow>();
                foreach (var tablerow in firstColumnTable.Elements<TableRow>())
                {

                    //if (tablerow.Descendants<Text>().Select(x => string.IsNullOrWhiteSpace(x.Text)).Count() >= 3)
                    if (string.IsNullOrWhiteSpace(tablerow.InnerText))
                    {
                        rowsToDelete.Add(tablerow);
                    }
                }

                foreach (var r in rowsToDelete)
                {
                    firstColumnTable.RemoveChild(r);
                }
                #endregion
                #region Second Column
                TableCell secondColumnCell = row.Elements<TableCell>().ElementAt(1);
                Table secondColumnTable = secondColumnCell.Elements<Table>().First();
                i = 0;
                foreach (var category in columns[1])
                {
                    var categoryRow = secondColumnTable.Elements<TableRow>().ElementAt(i);
                    var categoryCell1 = categoryRow.Elements<TableCell>().ElementAt(0);
                    var categoryCell2 = categoryRow.Elements<TableCell>().ElementAt(1);
                    var categoryCell3 = categoryRow.Elements<TableCell>().ElementAt(2);

                    categoryCell1.RemoveAllChildren();
                    categoryCell2.RemoveAllChildren();
                    categoryCell3.RemoveAllChildren();

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
                    foreach (var product in category.IncludedProducts)
                    {
                        try
                        {
                            var tableRow = secondColumnTable.Elements<TableRow>().ElementAt(i);
                            var firstCell = tableRow.Elements<TableCell>().ElementAt(0);
                            var secondCell = tableRow.Elements<TableCell>().ElementAt(1);
                            var thirdCell = tableRow.Elements<TableCell>().ElementAt(2);

                            firstCell.RemoveAllChildren();
                            secondCell.RemoveAllChildren();
                            thirdCell.RemoveAllChildren();

                            firstCell.Append(StandardText(product.ProductTariffName));
                            if (product.ABV == 0)
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

                rowsToDelete = new List<TableRow>();
                foreach (var tablerow in secondColumnTable.Elements<TableRow>())
                {
                    if (string.IsNullOrWhiteSpace(tablerow.InnerText))
                    {
                        rowsToDelete.Add(tablerow);
                    }
                }

                foreach (var r in rowsToDelete)
                {
                    secondColumnTable.RemoveChild(r);
                }
                #endregion
                #region Third Column
                TableCell thirdColumnCell = row.Elements<TableCell>().ElementAt(2);
                Table thirdColumnTable = thirdColumnCell.Elements<Table>().First();
                i = 0;
                foreach (var category in columns[2])
                {
                    var categoryRow = thirdColumnTable.Elements<TableRow>().ElementAt(i);

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
                    foreach (var product in category.IncludedProducts)
                    {
                        try
                        {
                            var tableRow = thirdColumnTable.Elements<TableRow>().ElementAt(i);
                            var firstCell = tableRow.Elements<TableCell>().ElementAt(0);
                            var secondCell = tableRow.Elements<TableCell>().ElementAt(1);
                            var thirdCell = tableRow.Elements<TableCell>().ElementAt(2);

                            firstCell.RemoveAllChildren();
                            secondCell.RemoveAllChildren();
                            thirdCell.RemoveAllChildren();

                            firstCell.Append(StandardText(product.ProductTariffName));
                            if (product.ABV == 0)
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

                rowsToDelete = new List<TableRow>();
                foreach (var tablerow in thirdColumnTable.Elements<TableRow>())
                {
                    if (string.IsNullOrWhiteSpace(tablerow.InnerText))
                    {
                        rowsToDelete.Add(tablerow);
                    }
                }

                foreach (var r in rowsToDelete)
                {
                    thirdColumnTable.RemoveChild(r);
                }
                #endregion
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
        
        public string GenerateSinglePageTariff(FullSalesArea fullSalesArea, string outputPath, bool rerun = false)
        {
            int totalLinesRequired = fullSalesArea.Categories.Sum(p => p.LinesRequired);
            if (rerun)
            {
                int count = 0;
                foreach (var cat in fullSalesArea.Categories)
                {
                    foreach (var p in cat.IncludedProducts)
                    {
                        if (p.ProductTariffName != null && p.ProductTariffName.Length > 15)
                        {
                            count++;
                        }
                    }
                }


                totalLinesRequired += count;
            }


            List<List<FullCategory>> columns;
            string templateLocation;
            if(totalLinesRequired < 70)
            {
                templateLocation = @"C:\Users\AdamM2\OneDrive - Zonal Retail Data Systems Limited\Desktop\TariffNotesNStuff\TEMPLATE2.docx";
                ColumnCount = 2;
                columns = SplitByColumns(fullSalesArea.Categories, 2);
            } else if (totalLinesRequired < 90)
            {
                templateLocation = @"C:\Users\AdamM2\OneDrive - Zonal Retail Data Systems Limited\Desktop\TariffNotesNStuff\TEMPLATE3.docx";
                ColumnCount = 3;
                columns = SplitByColumns(fullSalesArea.Categories, 3);
            } else
            {
                templateLocation = @"C:\Users\AdamM2\OneDrive - Zonal Retail Data Systems Limited\Desktop\TariffNotesNStuff\TEMPLATE4.docx";
                ColumnCount = 4;
                columns = SplitByColumns(fullSalesArea.Categories, 4);
            }

            
            File.Delete(outputPath);
            File.Copy(templateLocation, outputPath);
            using (WordprocessingDocument doc = WordprocessingDocument.Open(outputPath, true))
            {
                #region Header
                foreach (var headerPart in doc.MainDocumentPart.HeaderParts)
                {
                    //Gets the text in headers
                    foreach (var currentText in headerPart.RootElement.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>())
                    {
                        currentText.Text = currentText.Text.Replace("[TITLE]", fullSalesArea.TariffName);
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
                        if (!category.IncludedProducts.Any()) continue;
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
                        foreach (var product in category.IncludedProducts)
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
                                if (product.ABV == 0)
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


        public string GenerateMultiPageTariff(FullSalesArea fullSalesArea, string outputPath)
        {
            var templateLocation = @"C:\Users\AdamM2\OneDrive - Zonal Retail Data Systems Limited\Desktop\TariffNotesNStuff\TEMPLATE2M.docx";
            File.Delete(outputPath);
            File.Copy(templateLocation, outputPath);
            var columns = SplitByColumns(fullSalesArea.Categories, 2);
            using (WordprocessingDocument doc = WordprocessingDocument.Open(outputPath, true))
            {
                #region Header
                foreach (var headerPart in doc.MainDocumentPart.HeaderParts)
                {
                    //Gets the text in headers
                    foreach (var currentText in headerPart.RootElement.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>())
                    {
                        currentText.Text = currentText.Text.Replace("[TITLE]", fullSalesArea.TariffName);
                    }
                }
                #endregion
                #region Tables

                Table table = doc.MainDocumentPart.Document.Body.Elements<Table>().First();
                TableRow row = table.Elements<TableRow>().ElementAt(0);

                int columnIterator = 0;
                foreach(var columnCell in row.Elements<TableCell>())
                {
                    Table columnTable = columnCell.Elements<Table>().First();
                    int i = 0;
                    foreach (var category in columns[columnIterator])
                    {
                        if (!category.IncludedProducts.Any()) continue;
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
                        foreach (var product in category.IncludedProducts)
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
                                if (product.ABV == 0)
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
                case 2:
                    fontSize = 23;
                    break;
                case 3:
                    fontSize = 20;
                    break;
                case 4:
                    fontSize = 15;
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
                case 2:
                    fontSize = 22;
                    break;
                case 3:
                    fontSize = 17;
                    break;
                case 4:
                    fontSize = 12;
                    break;
            }
            FontSize fs = new FontSize() { Val = fontSize.ToString() };

            return new Paragraph(new Run(new RunProperties(italic, fs, unbold), new Text(v)));
        }

        public List<List<FullCategory>> SplitByColumns(List<FullCategory> list, int numberOfColumns)
        {
            List<List<FullCategory>> result = new List<List<FullCategory>>();
            int totalLines = list.Sum(x => x.LinesRequired);
            int averageLinesPerColumn = totalLines / numberOfColumns;

            int categoryCount = 0;
            for(int i = 0; i < numberOfColumns; i++)
            {
                int lineCount = 0;
                var colcats = new List<FullCategory>();
                while (lineCount < averageLinesPerColumn - list[categoryCount].LinesRequired)
                {
                    colcats.Add(list[categoryCount]);
                    lineCount += list[categoryCount].LinesRequired;
                    categoryCount++;
                    if (categoryCount >= list.Count) break;
                }
                result.Add(colcats);
            }

            return result;
        }
    }
}
