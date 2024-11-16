using ParserSpace;

namespace TestParser
{
    public class Parser_Errors
    {
        [Fact]
        public void Calculated_EmptyExpression_ReturnsErrorMessage()
        {
            // Arrange
            Parser parser = new Parser(5, 5);
            int col = 3;
            int row = 3;
            string expression = ""; // Валідний математичний вираз

            // Act
            string result = parser.Calculated(col, row, expression);

            // Assert
            Assert.Equal("", result); // Очікуємо результат ""
        }
        [Fact]
        public void Calculated_InvalidExpression_ReturnsErrorMessage()
        {
            // Arrange
            Parser parser = new Parser(8, 5);
            int col = 1;
            int row = 1;
            string expression = "15 + *0 > 3^(1+2)"; // Валідний математичний вираз

            // Act
            string result = parser.Calculated(col, row, expression);

            // Assert
            Assert.Equal("SYNTAX", result); // Очікуємо результат SYNTAX
        }
        [Fact]
        public void Calculated_DivisionByZero_ReturnsErrorMessage()
        {
            // Arrange
            Parser parser = new Parser(5, 5);
            int col = 2;
            int row = 4;
            string expression = "5/0"; // Валідний математичний вираз

            // Act
            string result = parser.Calculated(col, row, expression);

            // Assert
            Assert.Equal("DIVBYZERO", result); // Очікуємо результат DIVBYZERO
        }
    }
}
