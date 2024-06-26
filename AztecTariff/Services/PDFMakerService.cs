﻿using AztecTariff.Models;
using System.Reflection.Metadata;
using iTextSharp.text.pdf;
using iTextSharp.text;
using AztecTariffModels.Models;
using System.Reflection.Metadata.Ecma335;

namespace AztecTariff.Services
{
    public class PDFMakerService
    {
        Settings settings;
        
        string filename;

        public PDFMakerService(Settings _settings)
        {
            settings = _settings;
        }

        public string MakePDF(FullSalesArea salesArea, string _template, bool includeABV)
        {
            filename = Path.Combine(settings.PDFOuputFilesLocation, GenerateFileName(salesArea));

            switch (_template)
            {
                case "Single-Page":
                    MakeSinglePagePdf(salesArea, _template, includeABV);
                    break;
                case "Multi-Page":
                    MakeMultiPagePdf(salesArea, _template, includeABV);
                    break;
                case "Landscape Single-Page":
                    MakeSinglePageLandscapePdf(salesArea, _template, includeABV);
                    break;
                case "Landscape Multi-Page":
                    MakeMultiPageLandscapePdf(salesArea, _template, includeABV);
                    break;
            }

            return filename;
        }

        public string MakeSinglePagePdf(FullSalesArea salesArea, string _template, bool includeABV)
        {
            iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 10, 10, 10, 10);
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filename, FileMode.Create));
            document.Open();

            float fontSize = 8;


            int linesPerPage = CalculateLinesPerPage(PageSize.A4.Height, 72, 72, fontSize, fontSize * 1.2f);// - 2;
            var catColumns = DistributeCategoriesByPage(salesArea.Categories, linesPerPage);
            if (catColumns.Count == 1)
            {
                fontSize = 16;
            }
            if (catColumns.Count == 2)
            {
                fontSize = 10;
            }
            if (catColumns.Count == 3)
            {
                fontSize = 8;
            }
            if (catColumns.Count > 3)
            {
                fontSize = 8;
            }


            linesPerPage = CalculateLinesPerPage(PageSize.A4.Height, 72, 72, fontSize, fontSize * 1.2f) - 10;
            catColumns = DistributeCategoriesByPage(salesArea.Categories, linesPerPage);
            int columns = catColumns.Count;
            if (catColumns.Count == 1)
            {
                fontSize = 16;
            }
            if (catColumns.Count == 2)
            {
                fontSize = 10;
            }
            if (catColumns.Count == 3)
            {
                fontSize = 8;
            }
            if (catColumns.Count > 3)
            {
                fontSize = 8;
            }
            var messageFont = FontFactory.GetFont("Arial", fontSize + 2);
            document.Add(new Paragraph(salesArea.TariffName, messageFont));

            messageFont = FontFactory.GetFont("Arial", fontSize);
            document.Add(new Paragraph(salesArea.HeaderMessage, messageFont));


            AddTablesAndColumns(columns, document, salesArea, fontSize, includeABV, catColumns);

            return "";
        }

        public string MakeSinglePageLandscapePdf(FullSalesArea salesArea, string _template, bool includeABV)
        {
            iTextSharp.text.Document document = new iTextSharp.text.Document(PageSize.A4.Rotate(), 10, 10, 10, 10);
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filename, FileMode.Create));
            document.Open();

            float fontSize = 8;


            var messageFont = FontFactory.GetFont("Arial", fontSize + 2);
            document.Add(new Paragraph(salesArea.TariffName, messageFont));

            messageFont = FontFactory.GetFont("Arial", fontSize);
            document.Add(new Paragraph(salesArea.HeaderMessage, messageFont));

            int linesPerPage = CalculateLinesPerPage(PageSize.A4.Height, 72, 72, fontSize, fontSize * 1.2f) - 2;
            var catColumns = DistributeCategoriesByPage(salesArea.Categories, linesPerPage);


            int columns = catColumns.Count;


            AddTablesAndColumns(columns, document, salesArea, fontSize, includeABV, catColumns);

            return "";
        }

        public string MakeMultiPagePdf(FullSalesArea salesArea, string _template, bool includeABV)
        {
            iTextSharp.text.Document document = new iTextSharp.text.Document(PageSize.A4, 10, 10, 10, 10);
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filename, FileMode.Create));
            document.Open();

            float fontSize = 8;


            var messageFont = FontFactory.GetFont("Arial", fontSize + 2);
            document.Add(new Paragraph(salesArea.TariffName, messageFont));

            messageFont = FontFactory.GetFont("Arial", fontSize);
            document.Add(new Paragraph(salesArea.HeaderMessage, messageFont));

            //int linesPerPage = CalculateLinesPerPage(PageSize.A4.Height, 72, 72, fontSize, fontSize * 1.2f) - 2;



            int columns = 2;
            var catColumns = SplitList(salesArea.Categories);
            AddTablesAndColumns(columns, document, salesArea, fontSize, includeABV, catColumns);



            return "";
        }

        //void AddTablesAndColumns(int columns, iTextSharp.text.Document document, FullSalesArea salesArea, float fontSize, bool includeABV, List<List<FullCategory>> sortedCats)
        //{
        //    PdfPTable table = new PdfPTable(columns);
        //    table.TotalWidth = document.PageSize.Width - 20;
        //    table.LockedWidth = true;
        //    table.HorizontalAlignment = Element.ALIGN_CENTER;


        //    PdfPCell[] columnPlaceholders = new PdfPCell[columns];
        //    for (int i = 0; i < columns; i++)
        //    {
        //        columnPlaceholders[i] = new PdfPCell();
        //        columnPlaceholders[i].Border = PdfPCell.NO_BORDER;
        //        columnPlaceholders[i].AddElement(new Paragraph(""));
        //    }

        //    int currentColumn = 0;



        //    foreach (var categorylist in sortedCats)
        //    //foreach (var category in salesArea.Categories)
        //    {
        //        foreach (var category in categorylist)
        //        {
        //            if (category.IncludedInPDFProducts.Count == 0) continue;
        //            PdfPTable categoryTable = new PdfPTable(1);
        //            categoryTable.DefaultCell.Border = PdfPCell.NO_BORDER;

        //            categoryTable.AddCell(new PdfPCell(new Phrase(category.TariffCategory, FontFactory.GetFont("Arial", fontSize, Font.BOLD))) { Border = PdfPCell.NO_BORDER });

        //            PdfPCell categoryCell = new PdfPCell();
        //            categoryCell.Border = PdfPCell.NO_BORDER;



        //            if (category.IsSummarized)
        //            {
        //                var p1Cell = new PdfPCell();
        //                p1Cell.Border = PdfPCell.NO_BORDER;
        //                p1Cell.AddElement(new Phrase($"{category.SummarizedCategory.SummaryDescription} {category.SummarizedCategory.MinPrice} -  {category.SummarizedCategory.MaxPrice}", FontFactory.GetFont("Arial", fontSize - 1)));
        //                categoryTable.AddCell(p1Cell);
        //            }
        //            else
        //            {
        //                PdfPTable productTable = new PdfPTable(new float[] { 5, 3, 3 });
        //                productTable.WidthPercentage = 100;

        //                foreach (var product in category.IncludedInPDFProducts)
        //                {
        //                    var p1Cell = new PdfPCell();
        //                    p1Cell.Border = PdfPCell.NO_BORDER;
        //                    p1Cell.AddElement(new Phrase(product.ProductTariffName, FontFactory.GetFont("Arial", fontSize - 2)));
        //                    productTable.AddCell(p1Cell);

        //                    var p2Cell = new PdfPCell();
        //                    p2Cell.Border = PdfPCell.NO_BORDER;
        //                    p2Cell.AddElement(new Phrase($"£{product.Price:0.00}", FontFactory.GetFont("Arial", fontSize - 2)));
        //                    productTable.AddCell(p2Cell);

        //                    var p3Cell = new PdfPCell();
        //                    p3Cell.Border = PdfPCell.NO_BORDER;

        //                    if (includeABV && product.ABV != 0)
        //                    {
        //                        p3Cell.AddElement(new Phrase($"{product.ABV}%", FontFactory.GetFont("Arial", fontSize - 3)));
        //                    }
        //                    else
        //                    {
        //                        p3Cell.AddElement(new Phrase($"", FontFactory.GetFont("Arial", fontSize - 2)));
        //                    }
        //                    productTable.AddCell(p3Cell);
        //                }
        //                categoryTable.AddCell(productTable);
        //            }
        //            if (currentColumn >= columns)
        //            {
        //                currentColumn = 0;
        //            }

        //            columnPlaceholders[currentColumn].AddElement(categoryTable);
        //            if(category == categorylist.Last()) 
        //                currentColumn++;
        //        }
        //    }

        //    foreach (var placeholder in columnPlaceholders)
        //    {
        //        table.AddCell(placeholder);
        //    }

        //    document.Add(table);


        //    PdfPTable tbfooter = new PdfPTable(3);
        //    tbfooter.TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
        //    tbfooter.DefaultCell.Border = 0;
        //    tbfooter.AddCell(new Paragraph());
        //    tbfooter.AddCell(new Paragraph());
        //    var _cell2 = new PdfPCell(new Paragraph(new Chunk(salesArea.FooterMessage, FontFactory.GetFont("Arial", fontSize, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK))));
        //    _cell2.HorizontalAlignment = Element.ALIGN_BOTTOM;
        //    _cell2.VerticalAlignment= Element.ALIGN_BOTTOM;
        //    _cell2.Border = 0;
        //    tbfooter.AddCell(_cell2);
        //    tbfooter.AddCell(new Paragraph());
        //    tbfooter.AddCell(new Paragraph());
        //    float[] widths1 = new float[] { 20f, 20f, 60f };
        //    tbfooter.SetWidths(widths1);
        //    document.Add(tbfooter);


        //    document.Close();
        //}

        //void AddTablesAndColumns(int columns, iTextSharp.text.Document document, FullSalesArea salesArea, float fontSize, bool includeABV, List<List<FullCategory>> sortedCats)
        //{
        //    PdfPTable table = new PdfPTable(columns);
        //    table.TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
        //    table.LockedWidth = true;
        //    table.HorizontalAlignment = Element.ALIGN_CENTER;

        //    foreach (var categorylist in sortedCats)
        //    {
        //        foreach (var category in categorylist)
        //        {
        //            if (category.IncludedInPDFProducts.Count == 0) continue;

        //            PdfPTable categoryTable = CreateCategoryTable(fontSize, category, includeABV);
        //            PdfPCell categoryCell = new PdfPCell(categoryTable);
        //            categoryCell.Border = PdfPCell.NO_BORDER;

        //            table.AddCell(categoryCell);
        //        }
        //    }

        //    document.Add(table);

        //    AddFooter(document, salesArea.FooterMessage, fontSize);

        //    document.Close();
        //}

        //PdfPTable CreateCategoryTable(float fontSize, FullCategory category, bool includeABV)
        //{
        //    PdfPTable categoryTable = new PdfPTable(1);
        //    categoryTable.DefaultCell.Border = PdfPCell.NO_BORDER;

        //    categoryTable.AddCell(new PdfPCell(new Phrase(category.TariffCategory, FontFactory.GetFont("Arial", fontSize, Font.BOLD))) { Border = PdfPCell.NO_BORDER });

        //    if (category.IsSummarized)
        //    {
        //        var p1Cell = new PdfPCell();
        //        p1Cell.Border = PdfPCell.NO_BORDER;
        //        p1Cell.AddElement(new Phrase($"{category.SummarizedCategory.SummaryDescription} {category.SummarizedCategory.MinPrice} -  {category.SummarizedCategory.MaxPrice}", FontFactory.GetFont("Arial", fontSize - 1)));
        //        categoryTable.AddCell(p1Cell);
        //    }
        //    else
        //    {
        //        PdfPTable productTable = new PdfPTable(new float[] { 5, 3, 3 });
        //        productTable.WidthPercentage = 100;

        //        foreach (var product in category.IncludedInPDFProducts)
        //        {
        //            var p1Cell = new PdfPCell();
        //            p1Cell.Border = PdfPCell.NO_BORDER;
        //            p1Cell.AddElement(new Phrase(product.ProductTariffName, FontFactory.GetFont("Arial", fontSize - 2)));
        //            productTable.AddCell(p1Cell);

        //            var p2Cell = new PdfPCell();
        //            p2Cell.Border = PdfPCell.NO_BORDER;
        //            p2Cell.AddElement(new Phrase($"£{product.Price:0.00}", FontFactory.GetFont("Arial", fontSize - 2)));
        //            productTable.AddCell(p2Cell);

        //            var p3Cell = new PdfPCell();
        //            p3Cell.Border = PdfPCell.NO_BORDER;

        //            if (includeABV && product.ABV != 0)
        //            {
        //                p3Cell.AddElement(new Phrase($"{product.ABV}%", FontFactory.GetFont("Arial", fontSize - 3)));
        //            }
        //            else
        //            {
        //                p3Cell.AddElement(new Phrase($"", FontFactory.GetFont("Arial", fontSize - 2)));
        //            }
        //            productTable.AddCell(p3Cell);
        //        }
        //        categoryTable.AddCell(productTable);
        //    }

        //    return categoryTable;
        //}

        //void AddFooter(iTextSharp.text.Document document, string footerMessage, float fontSize)
        //{
        //    PdfPTable tbfooter = new PdfPTable(3);
        //    tbfooter.TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
        //    tbfooter.DefaultCell.Border = 0;
        //    tbfooter.AddCell(new Paragraph());
        //    tbfooter.AddCell(new Paragraph());
        //    var _cell2 = new PdfPCell(new Paragraph(new Chunk(footerMessage, FontFactory.GetFont("Arial", fontSize, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK))));
        //    _cell2.HorizontalAlignment = Element.ALIGN_BOTTOM;
        //    _cell2.VerticalAlignment = Element.ALIGN_BOTTOM;
        //    _cell2.Border = 0;
        //    tbfooter.AddCell(_cell2);
        //    tbfooter.AddCell(new Paragraph());
        //    tbfooter.AddCell(new Paragraph());
        //    float[] widths1 = new float[] { 20f, 20f, 60f };
        //    tbfooter.SetWidths(widths1);
        //    document.Add(tbfooter);
        //}


        void AddTablesAndColumns(int columns, iTextSharp.text.Document document, FullSalesArea salesArea, float fontSize, bool includeABV, List<List<FullCategory>> sortedCats)
        {
            List<List<FullCategory>> columnCategories = DistributeCategoriesIntoColumns(columns, sortedCats);

            PdfPTable table = new PdfPTable(columns);
            table.TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
            table.LockedWidth = true;
            table.HorizontalAlignment = Element.ALIGN_CENTER;
            table.SplitLate = false; 
            table.SplitRows = true;

            foreach (var column in columnCategories)
            {
                PdfPCell columnCell = new PdfPCell();
                columnCell.Border = PdfPCell.NO_BORDER;
                columnCell.Padding = 5;

                foreach (var category in column)
                {
                    if (category.IncludedInPDFProducts.Count == 0) continue;

                    PdfPTable categoryTable = CreateCategoryTable(fontSize, category, includeABV);
                    columnCell.AddElement(categoryTable);
                }

                table.AddCell(columnCell);
            }


            //for the maximum number of rows
            //if caolumncat.num > i add columncat[0]cat[i]
            //add columncat[1]cat[i]
            //add columncat[2]cat[i]

            //int maxRows = columnCategories.MaxBy(x => x.Count).Count;

            //for(int i = 0; i < maxRows; i++)
            //{
            //    PdfPCell columnCell = new PdfPCell();
            //    columnCell.Border = PdfPCell.NO_BORDER;
            //    columnCell.Padding = 5;

            //    for (int j = 0; j < columnCategories.Count; j++)
            //    {
            //        if (i < columnCategories[j].Count && columnCategories[j][i].IncludedInPDFProducts.Count != 0)
            //        {
            //            PdfPTable categoryTable = CreateCategoryTable(fontSize, columnCategories[j][i], includeABV);
            //            columnCell.AddElement(categoryTable);
            //        }
            //    }
            //    table.AddCell(columnCell);
            //}



            document.Add(table);

            AddFooter(document, salesArea.FooterMessage, fontSize);

            document.Close();
        }

        List<List<FullCategory>> DistributeCategoriesIntoColumns(int columns, List<List<FullCategory>> sortedCats)
        {
            List<List<FullCategory>> columnCategories = new List<List<FullCategory>>();
            for (int i = 0; i < columns; i++)
            {
                columnCategories.Add(new List<FullCategory>());
            }

            int totalLines = 0;
            foreach (var categoryList in sortedCats)
            {
                foreach (var category in categoryList)
                {
                    totalLines += category.LinesRequired;
                }
            }

            int averageLinesPerColumn = totalLines / columns;
            int[] columnLineCounts = new int[columns];

            foreach (var categoryList in sortedCats)
            {
                foreach (var category in categoryList)
                {
                    int minColumnIndex = Array.IndexOf(columnLineCounts, columnLineCounts.Min());
                    columnCategories[minColumnIndex].Add(category);
                    columnLineCounts[minColumnIndex] += category.LinesRequired;
                }
            }

            return columnCategories;
        }

        PdfPTable CreateCategoryTable(float fontSize, FullCategory category, bool includeABV)
        {
            PdfPTable categoryTable = new PdfPTable(1);
            categoryTable.DefaultCell.Border = PdfPCell.NO_BORDER;
            categoryTable.DefaultCell.Padding = 5; 

            PdfPCell categoryTitleCell = new PdfPCell(new Phrase(category.TariffCategory, FontFactory.GetFont("Arial", fontSize, Font.BOLD)));
            categoryTitleCell.Border = PdfPCell.NO_BORDER;
            categoryTitleCell.Padding = 5; 
            categoryTable.AddCell(categoryTitleCell);

            if (category.IsSummarized)
            {
                var summaryCell = new PdfPCell(new Phrase($"{category.SummarizedCategory.SummaryDescription} £{category.SummarizedCategory.MinPrice.ToString("0.00")} - £{category.SummarizedCategory.MaxPrice.ToString("0.00")}", FontFactory.GetFont("Arial", fontSize - 1)));
                summaryCell.Border = PdfPCell.NO_BORDER;
                summaryCell.Padding = 5; 
                categoryTable.AddCell(summaryCell);
            }
            else
            {
                PdfPTable productTable = new PdfPTable(new float[] { 5, 3, 3 });
                productTable.WidthPercentage = 100;
                productTable.DefaultCell.Border = PdfPCell.NO_BORDER;
                productTable.DefaultCell.Padding = 5;

                foreach (var product in category.IncludedInPDFProducts)
                {
                    var productNameCell = new PdfPCell(new Phrase(product.ProductTariffName, FontFactory.GetFont("Arial", fontSize - 2)));
                    productNameCell.Border = PdfPCell.NO_BORDER;
                    productNameCell.Padding = 5; 
                    productTable.AddCell(productNameCell);

                    var productPriceCell = new PdfPCell(new Phrase($"£{product.Price:0.00}", FontFactory.GetFont("Arial", fontSize - 2)));
                    productPriceCell.Border = PdfPCell.NO_BORDER;
                    productPriceCell.Padding = 5;
                    productTable.AddCell(productPriceCell);

                    var productABVCell = new PdfPCell();
                    productABVCell.Border = PdfPCell.NO_BORDER;
                    productABVCell.Padding = 5; 

                    if (includeABV && product.ABV != 0)
                    {
                        productABVCell.AddElement(new Phrase($"{product.ABV}%", FontFactory.GetFont("Arial", fontSize - 3)));
                    }
                    else
                    {
                        productABVCell.AddElement(new Phrase($"", FontFactory.GetFont("Arial", fontSize - 2)));
                    }
                    productTable.AddCell(productABVCell);
                }
                categoryTable.AddCell(productTable);
            }

            return categoryTable;
        }

        void AddFooter(iTextSharp.text.Document document, string footerMessage, float fontSize)
        {
            PdfPTable tbfooter = new PdfPTable(3);
            tbfooter.TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
            tbfooter.DefaultCell.Border = 0;
            tbfooter.DefaultCell.Padding = 5; 
            tbfooter.AddCell(new Paragraph());
            tbfooter.AddCell(new Paragraph());
            var _cell2 = new PdfPCell(new Paragraph(new Chunk(footerMessage, FontFactory.GetFont("Arial", fontSize, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK))));
            _cell2.HorizontalAlignment = Element.ALIGN_BOTTOM;
            _cell2.VerticalAlignment = Element.ALIGN_BOTTOM;
            _cell2.Border = 0;
            _cell2.Padding = 5; 
            tbfooter.AddCell(_cell2);
            tbfooter.AddCell(new Paragraph());
            tbfooter.AddCell(new Paragraph());
            float[] widths1 = new float[] { 20f, 20f, 60f };
            tbfooter.SetWidths(widths1);
            document.Add(tbfooter);
        }



        public List<List<T>> SplitList<T>(List<T> originalList)
        {
            int midPoint = (originalList.Count + 1) / 2; // Middle point, rounded up if odd
            var list1 = new List<T>(originalList.GetRange(0, midPoint));
            var list2 = new List<T>(originalList.GetRange(midPoint, originalList.Count - midPoint));
            return new List<List<T>>
            {
                list1,
                list2
            };
        }

        public static List<List<FullCategory>> DistributeCategoriesByPage(List<FullCategory> categories, int linesPerPage)
        {
            linesPerPage = linesPerPage + 10;

            List<List<FullCategory>> pages = new List<List<FullCategory>>();
            List<FullCategory> currentPage = new List<FullCategory>();
            int currentLines = 0;

            foreach (var category in categories)
            {
                if (currentLines + category.LinesRequired > linesPerPage)
                {
                    pages.Add(currentPage);
                    currentPage = new List<FullCategory>();
                    currentLines = 0;
                }

                currentPage.Add(category);
                currentLines += category.LinesRequired;
            }

            if (currentPage.Count > 0)
            {
                pages.Add(currentPage);
            }

            return pages;
        }

        public static int CalculateLinesPerPage(float pageSizeHeight, float marginTop, float marginBottom, float fontSize, float leading)
        {
            float usableHeight = 700;// pageSizeHeight - marginTop - marginBottom;
            int linesPerPage = (int)(usableHeight / leading);

            return linesPerPage;
        }

        public string MakeMultiPageLandscapePdf(FullSalesArea salesArea, string v1, bool includeABV)
        {
            iTextSharp.text.Document document = new iTextSharp.text.Document(PageSize.A4.Rotate(), 10, 10, 10, 10);
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filename, FileMode.Create));
            document.Open();

            float fontSize = 8;


            var messageFont = FontFactory.GetFont("Arial", fontSize + 2);
            document.Add(new Paragraph(salesArea.TariffName, messageFont));

            messageFont = FontFactory.GetFont("Arial", fontSize);
            document.Add(new Paragraph(salesArea.HeaderMessage, messageFont));

            int linesPerPage = CalculateLinesPerPage(PageSize.A4.Height, 72, 72, fontSize, fontSize * 1.2f) - 15;
            var catColumns = DistributeCategoriesByPage(salesArea.Categories, linesPerPage);


            int columns = catColumns.Count;

            AddTablesAndColumns(columns, document, salesArea, fontSize, includeABV, catColumns);

            return "";
        }

        public string GenerateFileName(FullSalesArea salesArea)
        {
            var filename = $"{salesArea.TariffName}{DateTime.Now.ToString("ddMMyy")}.pdf";
            return filename;
        }

    }
}
