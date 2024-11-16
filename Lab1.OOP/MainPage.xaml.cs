using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Xaml;
using System;
using System.Collections.Generic;
using System.Numerics;
using CommunityToolkit.Maui.Storage;
using Grid = Microsoft.Maui.Controls.Grid;
using ParserSpace;

namespace Lab1.OOP
{
    public partial class MainPage : ContentPage
    {
        const int CountColumn = 20; // кількість стовпчиків (A to Z)
        const int CountRow = 50; // кількість рядків
        Parser parser = new Parser(CountColumn, CountRow);

        public MainPage()
        {
            InitializeComponent();
            CreateGrid();
        }
        //створення таблиці
        private void CreateGrid()
        {
            AddColumnsAndColumnLabels();
            AddRowsAndCellEntries();
        }
        private void AddColumnsAndColumnLabels()
        {
            // Додати стовпці та підписи для стовпців
            grid.RowDefinitions.Add(new RowDefinition());
            for (int col = 0; col < CountColumn + 1; col++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                if (col > 0)
                {
                    var label = new Label
                    {
                        Text = GetColumnName(col),
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center
                    };
                    Grid.SetRow(label, 0);
                    Grid.SetColumn(label, col);
                    grid.Children.Add(label);
                }
            }
        }
        private void AddRowsAndCellEntries()
        {
            // Додати рядки, підписи для рядків та комірки
            for (int row = 1; row < CountRow + 1; row++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
                // Додати підпис для номера рядка
                var label = new Label
                {
                    Text = (row).ToString(),
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                };
                Grid.SetRow(label, row);
                Grid.SetColumn(label, 0);
                grid.Children.Add(label);
                // Додати комірки (Entry) для вмісту
                for (int col = 1; col < CountColumn + 1; col++)
                {
                    var entry = new Entry
                    {
                        Text = "",
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center
                    };
                    entry.Unfocused += Entry_Unfocused; // обробник події Unfocused
                    entry.Focused += Entry_Focused; // обробник події Focused
                    Grid.SetRow(entry, row);
                    Grid.SetColumn(entry, col);
                    grid.Children.Add(entry);
                }
            }
        }

        private string GetColumnName(int colIndex)
        {
            int dividend = colIndex;
            string columnName = string.Empty;
            while (dividend > 0)
            {
                int modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }
            return columnName;
        }


        // викликається, коли користувач вийде зі зміненої клітинки(втратить фокус)
        private void Entry_Unfocused(object sender, FocusEventArgs e)
        {
            var entry = (Entry)sender;
            var row = Grid.GetRow(entry) - 1;
            var col = Grid.GetColumn(entry) - 1;
            var content = entry.Text;
            entry.Text = parser.Calculated(col, row, content);
            textInput.Text = "";
            AddressVerification(row, col);


        }

        private void Entry_Focused(object sender, FocusEventArgs e)
        {
            var entry = (Entry)sender;
            var row = Grid.GetRow(entry) - 1;
            var col = Grid.GetColumn(entry) - 1;
            entry.Text = parser.ReturnExp(col, row);
            textInput.Text = parser.ReturnExp(col, row);
        }


        // Обробка кнопки "Зберегти"
        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            string filePath = Path.Combine(FileSystem.Current.AppDataDirectory, "Table.txt");
            parser.SaveTable(filePath);

            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.WinUI, new[] { ".txt" } },
            { DevicePlatform.iOS, new[] { "public.text" } },
            { DevicePlatform.Android, new[] { "text/plain" } }
        });

            // Виклик провідника для вибору місця збереження файлу
            var fileResult = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Виберіть місце для збереження файлу",
                FileTypes = customFileType
            });

            if (fileResult != null)
            {
                // Отримання шляху збереження файлу та копіювання даних до нього
                string destinationPath = fileResult.FullPath;
                File.Copy(filePath, destinationPath, overwrite: true);
            }
        }

        // Обробка кнопки "Прочитати"
        private async void ReadButton_Clicked(object sender, EventArgs e)
        {
            // Налаштування для вибору текстових файлів
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.WinUI, new[] { ".txt" } },
            { DevicePlatform.iOS, new[] { "public.plain-text" } },
            { DevicePlatform.Android, new[] { "text/plain" } },
            { DevicePlatform.MacCatalyst, new[] { "public.plain-text" } }
        });

            var options = new PickOptions
            {
                PickerTitle = "Виберіть файл для читання",
                FileTypes = customFileType
            };

            // Вибір файлу
            var fileResult = await FilePicker.PickAsync(options);

            if (fileResult != null)
            {
                string filePath = fileResult.FullPath;
                int index_lines = 0;
                string[] lines = File.ReadAllLines(filePath);
                int Row = int.Parse(lines[index_lines++]);
                while (Row > grid.RowDefinitions.Count - 1)
                {
                    AddRowButton_Clicked(sender, e);
                }
                while (Row < grid.RowDefinitions.Count - 1)
                {
                    DeleteRowButton_Clicked(sender, e);
                }
                int Column = int.Parse(lines[index_lines++]);
                while (Column > grid.ColumnDefinitions.Count - 1)
                {
                    AddColumnButton_Clicked(sender, e);
                }
                while (Column < grid.ColumnDefinitions.Count - 1)
                {
                    DeleteColumnButton_Clicked(sender, e);
                }
                for (int i = 1; i < grid.RowDefinitions.Count; i++)
                {
                    for (int j = 1; j < grid.ColumnDefinitions.Count; j++)
                    {
                        foreach (var child in grid.Children)
                        {
                            if (grid.GetRow(child) == i && grid.GetColumn(child) == j)
                            {
                                if (child is Entry entry)
                                {
                                    entry.Text = parser.ReadCell(lines, i - 1, j - 1, ref index_lines);
                                }
                            }
                        }
                    }
                }



            }
        }

        private async void ExitButton_Clicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Підтвердження", "Ви дійсно хочете вийти ? ", "Так", "Ні");
            if (answer)
            {
                System.Environment.Exit(0);
            }
        }

        private async void HelpButton_Clicked(object sender, EventArgs e)
        {
            await DisplayAlert("Довідка", "Лабораторна робота 1. Студента Толстопятого Андрія", "OK");
        }

        private void DeleteRowButton_Clicked(object sender, EventArgs e)
        {
            if (grid.RowDefinitions.Count > 1)
            {
                int lastRowIndex = grid.RowDefinitions.Count - 1;
                var elementsInRow = grid.Children
                        .Where(child => grid.GetRow(child) == lastRowIndex)
                        .ToList();

                foreach (var element in elementsInRow)
                {
                    grid.Children.Remove(element);
                }
                grid.RowDefinitions.RemoveAt(lastRowIndex);

                parser.RemoveRow(lastRowIndex - 1);

                for (int i = 0; i < grid.ColumnDefinitions.Count - 1; i++)
                {
                    AddressVerification(lastRowIndex - 1, i);
                }
            }

        }

        private void AddRowButton_Clicked(object sender, EventArgs e)
        {
            int newRow = grid.RowDefinitions.Count;
            // Add a new row definition
            grid.RowDefinitions.Add(new RowDefinition());
            // Add label for the row number
            var label = new Label
            {
                Text = newRow.ToString(),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            Grid.SetRow(label, newRow);
            Grid.SetColumn(label, 0);
            grid.Children.Add(label);

            // Add entry cells for the new row
            for (int col = 1; col < grid.ColumnDefinitions.Count; col++)
            {
                var entry = new Entry
                {
                    Text = "",
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                };
                entry.Unfocused += Entry_Unfocused;
                entry.Focused += Entry_Focused;
                Grid.SetRow(entry, newRow);
                Grid.SetColumn(entry, col);
                grid.Children.Add(entry);
            }

            parser.AddRow(grid.ColumnDefinitions.Count - 1);
        }

        private void AddColumnButton_Clicked(object sender, EventArgs e)
        {
            int newColumn = grid.ColumnDefinitions.Count;
            // Add a new column definition
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            // Add label for the column number
            var label = new Label
            {
                Text = GetColumnName(newColumn),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            Grid.SetRow(label, 0);
            Grid.SetColumn(label, newColumn);
            grid.Children.Add(label);

            // Add entry cells for the new row
            for (int row = 1; row < grid.RowDefinitions.Count; row++)
            {
                var entry = new Entry
                {
                    Text = "",
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                };
                entry.Unfocused += Entry_Unfocused;
                entry.Focused += Entry_Focused;
                Grid.SetRow(entry, row);
                Grid.SetColumn(entry, newColumn);
                grid.Children.Add(entry);
            }

            parser.AddColumn(grid.RowDefinitions.Count - 1);
        }

        private void DeleteColumnButton_Clicked(object sender, EventArgs e)
        {
            if (grid.ColumnDefinitions.Count > 1)
            {
                int lastColumnIndex = grid.ColumnDefinitions.Count - 1;
                var elementsInRow = grid.Children
                        .Where(child => grid.GetColumn(child) == lastColumnIndex)
                        .ToList();

                foreach (var element in elementsInRow)
                {
                    grid.Children.Remove(element);
                }
                grid.ColumnDefinitions.RemoveAt(lastColumnIndex);

                parser.RemoveColumn(grid.RowDefinitions.Count - 1, lastColumnIndex - 1);

                for (int i = 0; i < grid.RowDefinitions.Count - 1; i++)
                {
                    AddressVerification(i, lastColumnIndex - 1);
                }
            }
        }

        private void AddressVerification(int Row, int Column)
        {
            string cell_name = GetColumnName(Column + 1);
            cell_name = cell_name + (Row + 1).ToString();
            for (int i = 0; i < grid.RowDefinitions.Count - 1; i++)
            {
                for (int j = 0; j < grid.ColumnDefinitions.Count - 1; j++)
                {
                    //if (i == Row && j == Column) continue;
                    if (parser.FindAddress(i, j, cell_name))
                    {
                        string cell_name_t = GetColumnName(j + 1);
                        cell_name_t = cell_name_t + (i + 1).ToString();
                        if (parser.FindAddress(Row, Column, cell_name_t))
                        {
                            foreach (var child in grid.Children)
                            {
                                if (grid.GetRow(child) == Row + 1 && grid.GetColumn(child) == Column + 1)
                                {
                                    if (child is Entry entry)
                                    {
                                        entry.Text = "LOPING";
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var child in grid.Children)
                            {
                                if (grid.GetRow(child) == i + 1 && grid.GetColumn(child) == j + 1)
                                {
                                    if (child is Entry entry)
                                    {
                                        entry.Text = parser.Calculated(j, i, parser.ReturnExp(j, i));
                                        AddressVerification(i, j);
                                    }
                                }
                            }
                        }

                    }

                }
            }
        }

    }
}


