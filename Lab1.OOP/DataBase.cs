using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserSpace
{
    public class DataBase
    {
        protected enum TypeCell { STRING, NUMBER, BOOL };
        protected enum Errors { SYNTAX, UNBALPARENS, NOEXP, DIVBYZERO, MINUSUNDERPOWER }; // помилки.
        protected struct Cell
        {
            public string expression;
            public double value;
            public TypeCell typeCell;
            public List<string> ListAdress;
            public Cell()
            {
                expression = "";
                value = 0.0;
                typeCell = TypeCell.STRING;
                ListAdress = new List<string>();
            }
        }
        protected List<List<Cell>> cells;

        public DataBase(int CountColumn, int CountRow)
        {
            cells = new List<List<Cell>>();
            for (int row = 0; row < CountRow; row++)
            {
                List<Cell> row_cell = new List<Cell>();
                for (int col = 0; col < CountColumn; col++)
                {
                    row_cell.Add(new Cell());
                }
                cells.Add(row_cell);
            }
        }

        public void AddRow(int CountColumn)
        {
            List<Cell> row_cell = new List<Cell>();
            for (int col = 0; col < CountColumn; col++)
            {
                row_cell.Add(new Cell());
            }
            cells.Add(row_cell);
        }

        public void RemoveRow(int lastRowIndex)
        {
            cells.RemoveAt(lastRowIndex);
        }

        public void AddColumn(int CountRow)
        {
            for (int row = 0; row < CountRow; row++)
            {
                cells[row].Add(new Cell());
            }
        }

        public void RemoveColumn(int CountRow, int lastColumnIndex)
        {
            for (int row = 0; row < CountRow; row++)
            {
                cells[row].RemoveAt(lastColumnIndex);
            }
        }

        public string ReturnExp(int Column, int Row)
        {
            return cells[Row][Column].expression;
        }

        public void SaveTable(string filePath)
        {
            File.WriteAllText(filePath, string.Empty);
            File.AppendAllText(filePath, cells.Count.ToString() + "\n");
            if(cells.Count == 0)
            {
                File.AppendAllText(filePath, "0\n");
            }
            else
            {
                File.AppendAllText(filePath, cells[0].Count.ToString() + "\n");
            }
            
            for (int i = 0; i < cells.Count; i++)
            {
                for(int j = 0;  j < cells[i].Count; j++)
                {
                    File.AppendAllText(filePath, cells[i][j].expression + "\n");
                    File.AppendAllText(filePath, cells[i][j].value.ToString() + "\n");
                    File.AppendAllText(filePath, cells[i][j].typeCell.ToString() + "\n");
                    string listAddress = "";
                    for(int k = 0; k < cells[i][j].ListAdress.Count; k++)
                    {
                        listAddress += cells[i][j].ListAdress[k] + " ";
                    }
                    File.AppendAllText(filePath, listAddress + "\n");
                }
            }
        }

        public string ReadCell(string[] lines, int Row, int Column, ref int index_lines)
        {
            Cell cell_t;
            cell_t.expression = lines[index_lines++];
            cell_t.value = int.Parse(lines[index_lines++]);
            switch(lines[index_lines++])
            {
                case "BOOL": cell_t.typeCell = TypeCell.BOOL; break;
                case "NUMBER": cell_t.typeCell = TypeCell.NUMBER; break;
                default: cell_t.typeCell = TypeCell.STRING; break;
            }
            string[] address = lines[index_lines++].Split(" ");
            cell_t.ListAdress = new List<string>();
            for (int i = 0; i < address.Length; i++)
                cell_t.ListAdress.Add(address[i]);
            cells[Row][Column] = cell_t;
            if(cell_t.typeCell == TypeCell.NUMBER)
            {
                return cell_t.value.ToString();
            }
            else if(cell_t.typeCell == TypeCell.BOOL)
            {
                if (cell_t.value == 1.0) return "true";
                else return "false";
            }
            else
            {
                if (cell_t.expression == "") return cell_t.expression;
                else return "Error";
            }
        }
    }
}
