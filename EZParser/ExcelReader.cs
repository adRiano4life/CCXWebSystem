using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GemBox.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NLog;
using WebStudio.Models;

namespace EZParser
{
    public class ExcelReader
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public static void ExcelRead()
        {
            try
            {
                Console.WriteLine($"{DateTime.Now} - Парсинг Excel начат");
                _logger.Info("Парсинг Excel начат");
                string connection = Program.DefaultConnection; // общая строка
                var optionsBuilder = new DbContextOptionsBuilder<WebStudioContext>();
                var options = optionsBuilder.UseNpgsql(connection).Options;

                using WebStudioContext _db = new WebStudioContext(options);
                List<Card> cards = _db.Cards.ToList();
                List<CardPosition> dbPositions = _db.Positions.ToList();

                foreach (var card in cards)
                {
                    // int indexStart = card.Number.IndexOf('-') + 1;
                    // string cardNumber = card.Number.Substring(indexStart, 7);
                    string[] subCardNumber = card.Number.Split("/");
                    string cardNumber = subCardNumber[0];

                    if (card.Positions is null && card.Initiator.Contains("Казахмыс"))
                    {
                        string rootDirName = @$"{Program.PathToFiles}/Excel"; //общий путь
                        
                        List<string> fileNames = new List<string>();

                        DirectoryInfo dirInfo = new DirectoryInfo(rootDirName);
                        foreach (var file in dirInfo.GetFiles())
                        {
                            if (file.Name.Contains(cardNumber))
                                fileNames.Add(file.Name);
                        }

                        foreach (var file in fileNames)
                        {
                            if (!string.IsNullOrEmpty(file) && !file.ToLower().Contains("dap"))
                            {
                                SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
                                SpreadsheetInfo.FreeLimitReached += (sender, e) => e.FreeLimitReachedAction = FreeLimitReachedAction.ContinueAsTrial;
                                
                                ExcelFile workbook = ExcelFile.Load(@$"{rootDirName}/{file}"); // общий путь
                                
                                int rowCount = 0, startRow = 0;
                                for (int sheetIndex = 0; sheetIndex < workbook.Worksheets.Count; sheetIndex++)
                                {
                                    ExcelWorksheet worksheet = workbook.Worksheets[sheetIndex];
                                    for (int rowIndex = 0; rowIndex < worksheet.Rows.Count; rowIndex++)
                                    {
                                        ExcelRow row = worksheet.Rows[rowIndex];
                                        for (int columnIndex = 0; columnIndex < row.AllocatedCells.Count; columnIndex++)
                                        {
                                            ExcelCell cell = row.Cells[columnIndex];
                                            if (cell.ValueType != CellValueType.Null &&
                                                cell.Value.ToString().ToLower().Contains("п/п"))
                                            {
                                                startRow = rowIndex + 2;
                                            }

                                            if (startRow <= rowIndex + 2 && cell.Name.Substring(0, 1) == "A") rowCount++;
                                            if (cell.ValueType != CellValueType.Null &&
                                                cell.Value.ToString().ToLower().Contains("итого"))
                                            {
                                                columnIndex = worksheet.Columns.Count;
                                                rowIndex = worksheet.Rows.Count;
                                            }
                                        }
                                    }
                                }

                                for (int i = startRow; i < rowCount; i++)
                                {
                                    CardPosition position = new CardPosition
                                    {
                                        CardId = card.Id,
                                        StockNumber = workbook.Worksheets.First().Cells[$"B{i}"].Value.ToString(),
                                        CodTNVED = workbook.Worksheets.First().Cells[$"C{i}"].Value?.ToString(),
                                        Name = workbook.Worksheets.First().Cells[$"D{i}"].Value.ToString(),
                                        Measure = workbook.Worksheets.First().Cells[$"E{i}"].Value.ToString(),
                                        Amount = Convert.ToInt64(workbook.Worksheets.First().Cells[$"F{i}"].Value),
                                        Currency = workbook.Worksheets.First().Cells[$"G{i}"].Value.ToString(),
                                        UnitPrice = Convert.ToInt64(workbook.Worksheets.First().Cells[$"H{i}"].Value),
                                        TotalPrice = Convert.ToInt64(workbook.Worksheets.First().Cells[$"I{i}"].Value),
                                        PaymentTerms = workbook.Worksheets.First().Cells[$"M{i}"].Value.ToString(),
                                        DeliveryTime = workbook.Worksheets.First().Cells[$"N{i}"].Value.ToString(),
                                        DeliveryTerms = workbook.Worksheets.First().Cells[$"O{i}"].Value.ToString()
                                    };
                                    if (dbPositions.FirstOrDefault(p => p.Name == position.Name) != position)
                                    {
                                        _db.Positions.Add(position);
                                        _db.SaveChanges();
                                        Console.WriteLine($"Позиция {position.Name} добавлена к карточке {card.Number}");
                                        _logger.Info($"Позиция {position.Name} добавлена к карточке {card.Number}");
                                    }
                                }
                            }
                        }
                    }
                }
                Console.WriteLine($"{DateTime.Now} - Парсинг Excel закончен");
                _logger.Info("Парсинг Excel закончен");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }

        }
    }
}