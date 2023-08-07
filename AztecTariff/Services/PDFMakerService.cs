using AztecTariff.Models;
using Blazorise;
using Telerik.Documents.Core.Fonts;
using Telerik.SvgIcons;
using Telerik.Windows.Documents.Flow.FormatProviders.Html;
using Telerik.Windows.Documents.Flow.Model;
using Telerik.Windows.Documents.Flow.Model.Editing;
using Telerik.Windows.Documents.Flow.Model.Styles;
using Table = Telerik.Windows.Documents.Flow.Model.Table;
using TableCell = Telerik.Windows.Documents.Flow.Model.TableCell;

namespace AztecTariff.Services
{
    public class PDFMakerService
    {
        RadFlowDocument document;
        RadFlowDocumentEditor editor;
        List<Product> products;
        public async Task<string> MakePdf(FullSalesArea pdfDocMod)
        {
            
            HtmlFormatProvider htmlProvider = new Telerik.Windows.Documents.Flow.FormatProviders.Html.HtmlFormatProvider();
            document = htmlProvider.Import(System.IO.File.ReadAllText(@"Data/test.html"));
            editor = new RadFlowDocumentEditor(document);
            editor.CharacterFormatting.FontSize.LocalValue = 24;
            editor.ParagraphFormatting.TextAlignment.LocalValue = Telerik.Windows.Documents.Flow.Model.Styles.Alignment.Center;
            editor.InsertText($"{pdfDocMod.TariffName}");

            Break br = editor.InsertBreak(BreakType.LineBreak);
            br.TextWrappingRestartLocation = TextWrappingRestartLocation.NextFullLine;
            br = editor.InsertBreak(BreakType.LineBreak);
            br.TextWrappingRestartLocation = TextWrappingRestartLocation.NextFullLine;

            editor.CharacterFormatting.FontSize.LocalValue = 16;

            var columns = SplitList(pdfDocMod.Categories, 35);

            var pages = SplitList(columns);

            //For each Page
            for (int i = 0; i < pages.Count; i++)
            {
                int curRow = 0;
                if (pages[i].Count == 1)
                {
                    MakeSingleColumn(pages[i][0]);
                    continue;
                }

                Table t = editor.InsertTable(35, 4);
                t.PreferredWidth = new TableWidthUnit(700);

                //For each column
                for (int j = 0; j < pages[i].Count; j++)
                {
                    //For each ProductCategory
                    for (int k = 0; k < pages[i][j].Count; k++)
                    {
                        //Add Title

                        Telerik.Windows.Documents.Flow.Model.TableCell cell = new Telerik.Windows.Documents.Flow.Model.TableCell(document);
                        var row = t.Rows[curRow];
                        if (j == 1 && row.Cells.Count == 4 && pages[i].Count > 1)
                        {

                            cell = new Telerik.Windows.Documents.Flow.Model.TableCell(document);
                            cell.PreferredWidth = new TableWidthUnit(300);
                            row.Cells.Add(cell);
                            cell = new Telerik.Windows.Documents.Flow.Model.TableCell(document);
                            cell.PreferredWidth = new TableWidthUnit(100);
                            row.Cells.Add(cell);
                        }
                        curRow++;
                        cell = new Telerik.Windows.Documents.Flow.Model.TableCell(document);
                        cell.Padding = new Telerik.Windows.Documents.Primitives.Padding(2);
                        cell.ColumnSpan = 2;
                        editor.CharacterFormatting.FontSize.LocalValue = 24;
                        var run = cell.Blocks.AddParagraph().Inlines.AddRun($"{pages[i][j][k].TariffCategory}");

                        run.FontWeight = FontWeights.Bold;
                        run.Underline.Pattern = UnderlinePattern.Single;
                        editor.CharacterFormatting.FontSize.LocalValue = 16;
                        row.Cells.Add(cell);

                        //For each product
                        foreach (var p in pages[i][j][k].IncludedProducts)
                        {
                            row = t.Rows[curRow];
                            row.Height = new TableRowHeight(Telerik.Windows.Documents.Flow.Model.Styles.HeightType.Exact);
                            if (j == 1 && row.Cells.Count == 4 && pages[i].Count != 1)
                            {

                                cell = new TableCell(document);
                                cell.PreferredWidth = new TableWidthUnit(300);
                                row.Cells.Add(cell);
                                cell = new TableCell(document);
                                cell.PreferredWidth = new TableWidthUnit(100);
                                row.Cells.Add(cell);
                            }
                            cell = new TableCell(document);
                            cell.PreferredWidth = new TableWidthUnit(300);
                            cell.Padding = new Telerik.Windows.Documents.Primitives.Padding(2);
                            var r = cell.Blocks.AddParagraph().Inlines.AddRun($"{p.ProductTariffName}");
                            row.Cells.Add(cell);

                            cell = new TableCell(document);
                            cell.PreferredWidth = new TableWidthUnit(100);
                            cell.Padding = new Telerik.Windows.Documents.Primitives.Padding(2);
                            cell.Blocks.AddParagraph().Inlines.AddRun($"£{p.Price.ToString("F")}");
                            row.Cells.Add(cell);
                            curRow++;
                        }
                    }
                    curRow = 0;


                }
                if (i < pages.Count - 1)
                {
                    editor.InsertBreak(BreakType.PageBreak);
                }
            }

            Telerik.Windows.Documents.Flow.FormatProviders.Pdf.PdfFormatProvider pdfProvider = new Telerik.Windows.Documents.Flow.FormatProviders.Pdf.PdfFormatProvider();
            byte[] pdfBytes = pdfProvider.Export(document);
            string filename = "tmptariff.pdf";
            await System.IO.File.WriteAllBytesAsync(@$"Data/{filename}", pdfBytes);
            return filename;
        }

        private void MakeSingleColumn(List<FullCategory> list)
        {
            var t = editor.InsertTable();
            foreach (var pc in list)
            {
                Telerik.Windows.Documents.Flow.Model.TableRow row = new Telerik.Windows.Documents.Flow.Model.TableRow(document);
                var cell = new TableCell(document);
                cell.Padding = new Telerik.Windows.Documents.Primitives.Padding(2);
                cell.ColumnSpan = 2;
                editor.CharacterFormatting.FontSize.LocalValue = 24;
                var run = cell.Blocks.AddParagraph().Inlines.AddRun($"{pc.TariffCategory}");

                run.FontWeight = FontWeights.Bold;
                run.Underline.Pattern = UnderlinePattern.Single;
                editor.CharacterFormatting.FontSize.LocalValue = 16;
                row.Cells.Add(cell);
                t.Rows.Add(row);
                foreach (var p in pc.Products)
                {
                    row = new Telerik.Windows.Documents.Flow.Model.TableRow(document);
                    cell = new TableCell(document);
                    cell.PreferredWidth = new TableWidthUnit(300);
                    cell.Padding = new Telerik.Windows.Documents.Primitives.Padding(2);
                    var r = cell.Blocks.AddParagraph().Inlines.AddRun($"{p.ProductTariffName}");
                    row.Cells.Add(cell);

                    cell = new TableCell(document);
                    cell.PreferredWidth = new TableWidthUnit(100);
                    cell.Padding = new Telerik.Windows.Documents.Primitives.Padding(2);
                    cell.Blocks.AddParagraph().Inlines.AddRun($"£{p.Price.ToString("F")}");
                    row.Cells.Add(cell);
                    t.Rows.Add(row);
                }
            }
        }

        private string GenerateFileName()
        {
            var r = new Random();
            var x = r.Next(100000, 999999);
            return $"tmpTarrif{x}.pdf";
        }

        public List<List<List<FullCategory>>> SplitList(List<List<FullCategory>> source)
        {
            return source
                    .Select((x, i) => new { Index = i, Value = x })
                    .GroupBy(x => x.Index / 2)
                    .Select(x => x.Select(v => v.Value).ToList())
                    .ToList();
        }


        public List<List<FullCategory>> SplitList(List<FullCategory> myList, int maxSize)
        {
            List<List<FullCategory>> newLists = new List<List<FullCategory>>();
            List<FullCategory> currentList = new List<FullCategory>();
            int currentSize = 0;

            foreach (FullCategory myClass in myList)
            {
                if (myClass.LinesRequired == 0) continue;
                if (currentSize + myClass.Products.Count + 2 > maxSize)
                {
                    newLists.Add(currentList);
                    currentList = new List<FullCategory>();
                    currentSize = 0;
                }

                currentList.Add(myClass);
                currentSize += myClass.LinesRequired;
            }
            newLists.Add(currentList);

            return newLists;
        }
    }
}
