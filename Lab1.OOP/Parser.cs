using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace ParserSpace
{
    public class Parser : DataBase
    {
        enum Types { NONE, DELIMITER, FUNCTION, Address, NUMBER }; //лексеми.
        string exp; // рядок виразу
        int expIdx; // поточний індекс у виразі
        string token; // поточна лексема
        Types tokType; // тип лексеми
        int col; // стовпчик поточної клітини
        int row; // рядок поточної клітини

        public Parser(int CountColumn, int CountRow) : base(CountColumn, CountRow)
        { }


        public bool FindAddress(int i, int j, string cell_name)
        {
            if(i >= cells.Count || j >= cells[0].Count) return false;
            for (int k = 0; k < cells[i][j].ListAdress.Count; k++)
            {
                if (cells[i][j].ListAdress[k] == cell_name)
                {
                    return true;
                }
            }
            return false;
        }

        public string Calculated(int Column, int Row, string expstr)
        {
            col = Column;
            row = Row;
            Cell cell_t = cells[row][col];
            cell_t.expression = expstr;
            cell_t.ListAdress = new List<string>();
            cells[row][col] = cell_t;
            try
            {
                cell_t.value = Evaluate(expstr);
                cell_t.typeCell = cells[row][col].typeCell;
                cells[row][col] = cell_t;

            }
            catch (Exception ex)
            {
                cell_t.typeCell = Parser.TypeCell.STRING;
                switch (ex.Message)
                {
                    case "SYNTAX": return "SYNTAX";
                    case "UNBALPARENS": return "UNBALPARENS";
                    case "NOEXP": if (cells[row][col].expression == "") return ""; else return "SYNTAX";
                    case "DIVBYZERO": return "DIVBYZERO";
                    case "MINUSUNDERPOWER": return "MINUSUNDERPOWER";
                    default: return "Problem";
                }
            }

            if (cells[row][col].typeCell == TypeCell.BOOL)
            {
                if (cells[row][col].value == 0.0) return "false";
                else return "true";
            }
            else
                return cells[row][col].value.ToString();
        }

        double Evaluate(string expstr)
        {
            double result;
            exp = expstr;
            expIdx = 0;
            GetToken();
            if (tokType == Types.NONE)
            {
                SyntaxErr(Errors.NOEXP);
                return 0.0;
            }
            EvalExp1(out result);
            if (token != "")
            {
                SyntaxErr(Errors.SYNTAX);
                return 0.0;
            }
            return result;
        }

        // Обробка функції not
        void EvalExp1(out double result)
        {
            if (token == "not")
            {
                GetToken();
                if (token != "(")
                {
                    SyntaxErr(Errors.SYNTAX);
                    result = 0.0;
                    return;
                }
                GetToken();
                EvalExp1(out result);
                if (token != ")")
                {
                    SyntaxErr(Errors.UNBALPARENS);
                    result = 0.0;
                    return;
                }
                GetToken();
                if (result == 0.0 && cells[row][col].typeCell == TypeCell.BOOL) result = 1.0;
                else if (result == 1.0 && cells[row][col].typeCell == TypeCell.BOOL) result = 0.0;
                else
                {
                    SyntaxErr(Errors.SYNTAX);
                    result = 0.0;
                    return;
                }
            }
            else if (tokType == Types.Address)
            {
                result = FindAddress(token);
                if (cells[row][col].typeCell == TypeCell.STRING)
                {
                    SyntaxErr(Errors.SYNTAX);
                    return;
                }
                else if(cells[row][col].typeCell == TypeCell.NUMBER)
                {
                    EvalExp2(out result);
                }
                else GetToken();
            }
            else
            {
                EvalExp2(out result);
            }
        }

        // Обробка операцій =, >, <
        void EvalExp2(out double result)
        {
            string op;
            double partialResult1;
            EvalExp3(out partialResult1);
            if ((op = token) == "=" || op == "<" || op == ">")
            {
                double partialResult2;
                GetToken();
                EvalExp3(out partialResult2);
                Cell cell_t = cells[row][col];
                cell_t.typeCell = TypeCell.BOOL;
                cells[row][col] = cell_t;
                switch (op)
                {
                    case "=": result = (partialResult1 == partialResult2) ? 1.0 : 0.0; break;
                    case "<": result = (partialResult1 < partialResult2) ? 1.0 : 0.0; break;
                    case ">": result = (partialResult1 > partialResult2) ? 1.0 : 0.0; break;
                    default: SyntaxErr(Errors.SYNTAX); result = 0.0; break;
                }
            }
            else
            {
                result = partialResult1;
                Cell cell_t = cells[row][col];
                cell_t.typeCell = TypeCell.NUMBER;
                cells[row][col] = cell_t;
            }

        }
        // Додавання або віднімання двох доданків
        void EvalExp3(out double result)
        {
            string op;
            double partialResult;

            EvalExp4(out result);
            while ((op = token) == "+" || op == "-")
            {
                GetToken();
                EvalExp4(out partialResult);
                switch (op)
                {
                    case "-":
                        result = result - partialResult;
                        break;
                    case "+":
                        result = result + partialResult;
                        break;
                }
            }
        }

        // Обчислення множення і ділення
        void EvalExp4(out double result)
        {
            string op;
            double partialResult;
            EvalExp5(out result);
            while ((op = token) == "*" || op == "/")
            {
                GetToken();
                EvalExp5(out partialResult);
                switch (op)
                {
                    case "*":
                        result = result * partialResult;
                        break;
                    case "/":
                        if (partialResult == 0.0)
                            SyntaxErr(Errors.DIVBYZERO);
                        result = result / partialResult;
                        break;
                }
            }
        }

        // Піднесення до степені 
        void EvalExp5(out double result)
        {
            double partialResult, ex;
            EvalExp6(out result);
            if (token == "^")
            {
                GetToken();
                EvalExp5(out partialResult);
                ex = result;
                if (partialResult < 0 && result == 0)
                {
                    SyntaxErr(Errors.DIVBYZERO);
                    return;
                }
                else if (result < 0)
                {
                    SyntaxErr(Errors.MINUSUNDERPOWER);
                }
                result = Math.Pow(result, partialResult);
            }

        }

        // Обчислення функцій mod i div
        void EvalExp6(out double result)
        {
            string op;
            double partialResult;
            if ((op = token) == "mod" || op == "div")
            {
                GetToken();
                if (token != "(")
                {
                    SyntaxErr(Errors.SYNTAX);
                    result = 0.0;
                    return;
                }
                GetToken();
                EvalExp3(out result);
                if(cells[row][col].typeCell == TypeCell.BOOL)
                {
                    SyntaxErr(Errors.SYNTAX);
                    result = 0.0;
                    return;
                }
                if (token != ";")
                {
                    SyntaxErr(Errors.SYNTAX);
                    result = 0.0;
                    return;
                }
                GetToken();
                EvalExp3(out partialResult);
                if (cells[row][col].typeCell == TypeCell.BOOL)
                {
                    SyntaxErr(Errors.SYNTAX);
                    result = 0.0;
                    return;
                }
                if (token != ")")
                {
                    SyntaxErr(Errors.UNBALPARENS);
                    result = 0.0;
                    return;
                }
                GetToken();
                if (partialResult == 0.0)
                    SyntaxErr(Errors.DIVBYZERO);
                switch (op)
                {
                    case "mod":
                        result = result % partialResult;
                        break;
                    case "div":
                        result = (int)(result / partialResult);
                        break;
                }
            }
            else
            {
                EvalExp7(out result);
            }
        }

        // Обчислення виразів в дужках
        void EvalExp7(out double result)
        {
            if ((token == "("))
            {
                GetToken();
                EvalExp3(out result);
                if (token != ")")
                    SyntaxErr(Errors.UNBALPARENS);
                GetToken();
            }
            else Atom(out result);

        }
        // Одержання значення числа, або змінної 
        void Atom(out double result)
        {
            switch (tokType)
            {
                case Types.NUMBER:
                    try
                    {
                        result = Double.Parse(token);
                    }
                    catch (FormatException)
                    {
                        result = 0.0;
                        SyntaxErr(Errors.SYNTAX);
                    }
                    GetToken();
                    break;
                case Types.Address:
                    result = FindAddress(token);
                    if (cells[row][col].typeCell != TypeCell.STRING)
                    {
                        GetToken();
                        break;
                    }
                    else
                    {
                        SyntaxErr(Errors.SYNTAX);
                        break;
                    }
                default:
                    result = 0.0;
                    SyntaxErr(Errors.SYNTAX);
                    break;
            }
        }

        // Повертаємо значення змінної
        double FindAddress(string address)
        {
            int Row = -1;
            int Column = -1;
            string nameCell = address;
            for (int i = 0; i < nameCell.Length; i++)
            {
                int int_char = (int)nameCell[i];
                if (!(int_char >= 65 && int_char <= 90) && !Char.IsDigit(nameCell[i]))
                {
                    SyntaxErr(Errors.SYNTAX);
                    return 0.0;
                }
                else if (int_char >= 65 && int_char <= 90)
                {
                    if (Row >= 0)
                    {
                        SyntaxErr(Errors.SYNTAX);
                        return 0.0;
                    }
                    Column = (Column + 1) * 26 + int_char - 65;
                }
                else
                {
                    if (Column == -1)
                    {
                        SyntaxErr(Errors.SYNTAX);
                        return 0.0;
                    }
                    if (Row == -1)
                    {
                        if (int_char == 48)
                        {
                            SyntaxErr(Errors.SYNTAX);
                            return 0.0;
                        }
                        Row++;
                    }
                    Row = Row * 10 + int_char - 48;
                }
            }
            if (Row == -1 || Column == -1)
            {
                SyntaxErr(Errors.SYNTAX);
                return 0.0;
            }
            Cell cell_t = cells[row][col];
            if ((cell_t.ListAdress.Count > 0 && cell_t.ListAdress[cell_t.ListAdress.Count - 1] != address) || cell_t.ListAdress.Count == 0)
            {
                cell_t.ListAdress.Add(address);
            }
            cells[row][col] = cell_t;

            if (Row > cells.Count || Column >= cells[0].Count)
            {
                SyntaxErr(Errors.SYNTAX);
                return 0.0;
            }
            Row--;

            cell_t.typeCell = cells[Row][Column].typeCell;
            cells[row][col] = cell_t;
            return cells[Row][Column].value;

        }


        // отримуємо наступну лексему
        void GetToken()
        {
            tokType = Types.NONE;
            token = "";
            if (expIdx == exp.Length) return; //кінець виразу
            // пропускаємо пробіл
            while (expIdx < exp.Length && Char.IsWhiteSpace(exp[expIdx])) ++expIdx;
            // Хвостовий пробіл 
            if (expIdx == exp.Length) return;
            if (IsDelim(exp[expIdx]))
            {
                token += exp[expIdx];
                expIdx++;
                tokType = Types.DELIMITER;
            }
            else if (IsFunc(exp[expIdx]))
            {
                if (exp[expIdx] == 'm')
                {
                    token += exp[expIdx];
                    expIdx++;
                    if (expIdx >= exp.Length || exp[expIdx] != 'o') SyntaxErr(Errors.SYNTAX);
                    token += exp[expIdx];
                    expIdx++;
                    if (expIdx >= exp.Length || exp[expIdx] != 'd') SyntaxErr(Errors.SYNTAX);
                    token += exp[expIdx];
                    expIdx++;
                    tokType = Types.FUNCTION;
                }
                else if (exp[expIdx] == 'n')
                {
                    token += exp[expIdx];
                    expIdx++;
                    if (expIdx >= exp.Length || exp[expIdx] != 'o') SyntaxErr(Errors.SYNTAX);
                    token += exp[expIdx];
                    expIdx++;
                    if (expIdx >= exp.Length || exp[expIdx] != 't') SyntaxErr(Errors.SYNTAX);
                    token += exp[expIdx];
                    expIdx++;
                    tokType = Types.FUNCTION;
                }
                else if (exp[expIdx] == 'd')
                {
                    token += exp[expIdx];
                    expIdx++;
                    if (expIdx >= exp.Length || exp[expIdx] != 'i') SyntaxErr(Errors.SYNTAX);
                    token += exp[expIdx];
                    expIdx++;
                    if (expIdx >= exp.Length || exp[expIdx] != 'v') SyntaxErr(Errors.SYNTAX);
                    token += exp[expIdx];
                    expIdx++;
                    tokType = Types.FUNCTION;
                }
            }
            else if ((int)exp[expIdx] >= 65 && (int)exp[expIdx] <= 90)
            {
                // Це ім'я клітинки?
                while (((int)exp[expIdx] >= 65 && (int)exp[expIdx] <= 90) || Char.IsDigit(exp[expIdx]))
                {
                    token += exp[expIdx];
                    expIdx++;
                    if (expIdx >= exp.Length) break;
                }
                tokType = Types.Address;
            }
            else if (Char.IsDigit(exp[expIdx]))
            {
                // Це число?
                while (!IsDelim(exp[expIdx]))
                {
                    token += exp[expIdx];
                    expIdx++;
                    if (expIdx >= exp.Length) break;
                }
                tokType = Types.NUMBER;
            }
        }
        // Метод повертає значення true,
        // якщо с - роздільник
        bool IsDelim(char c)
        {
            if (("+-/*^=<>();".IndexOf(c) != -1))
                return true;
            return false;
        }
        bool IsFunc(char c)
        {
            if (("mnd".IndexOf(c) != -1))
                return true;
            return false;
        }

        void SyntaxErr(Errors e)
        {
            throw new Exception(e.ToString());
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
    }
}
