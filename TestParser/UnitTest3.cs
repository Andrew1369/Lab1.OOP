using ParserSpace;

namespace TestParser
{
    public class Parser_Address
    {
        [Fact]
        public void Calculated_BoolAddress_ReturnsCorrectResult()
        {
            // Arrange
            Parser parser = new Parser(5, 5);
            int col = 3;
            int row = 3;
            string expression = "4>5"; 
            parser.Calculated(col, row, expression); // Утворюємо булеву клітинку
            col = 1;
            row = 1;
            expression = "D4"; // Валідний математичний вираз

            // Act
            string result = parser.Calculated(col, row, expression);

            // Assert
            Assert.Equal("false", result); // Очікуємо результат false
        }
        [Fact]
        public void Calculated_BoolsAddress_ReturnsCorrectResult()
        {
            // Arrange
            Parser parser = new Parser(8, 5);
            int col = 1;
            int row = 0;
            string expression = "15 + 3";
            parser.Calculated(col, row, expression); // Утворюємо в клітинці числове значення
            col = 0;
            row = 0;
            expression = "1 + 4*2";
            parser.Calculated(col, row, expression); // Утворюємо в клітинці числове значення
            col = 0;
            row = 1;
            expression = "A1*3 - B1"; // Валідний математичний вираз

            // Act
            string result = parser.Calculated(col, row, expression);

            // Assert
            Assert.Equal("9", result); // Очікуємо результат 9
        }
        [Fact]
        public void Calculated_BoolAddress_ReturnsErrorMessage()
        {
            // Arrange
            Parser parser = new Parser(8, 5);
            int col = 1;
            int row = 0;
            string expression = "15 + 3";
            parser.Calculated(col, row, expression); // Утворюємо в клітинці числове значення
            col = 0;
            row = 0;
            expression = "1 > 4*2";
            parser.Calculated(col, row, expression); // Утворюємо в клітинці булеве значення
            col = 0;
            row = 1;
            expression = "A1*3 - B1"; // Валідний математичний вираз

            // Act
            string result = parser.Calculated(col, row, expression);

            // Assert
            Assert.Equal("SYNTAX", result); // Очікуємо результат SYNTAX
        }
    }
}
