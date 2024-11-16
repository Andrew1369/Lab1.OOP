using ParserSpace;

namespace TestParser
{
    public class Parser_CorrectResult
    {
        [Fact]
        public void Calculated_ValidExpression_ReturnsCorrectResult1()
        {
            // Arrange
            Parser parser = new Parser(5, 5);
            int col = 2;
            int row = 4;
            string expression = "2 + 3^2 * 4 - 15"; // Валідний математичний вираз

            // Act
            string result = parser.Calculated(col, row, expression);

            // Assert
            Assert.Equal("23", result); // Очікуємо результат 23
        }
        [Fact]
        public void Calculated_ValidExpression_ReturnsCorrectResult2()
        {
            // Arrange
            Parser parser = new Parser(8, 5);
            int col = 1;
            int row = 1;
            string expression = "15 + 24*0 > 3^(1+2)"; // Валідний математичний вираз

            // Act
            string result = parser.Calculated(col, row, expression);

            // Assert
            Assert.Equal("false", result); // Очікуємо результат false
        }
        [Fact]
        public void Calculated_ValidExpression_ReturnsCorrectResult3()
        {
            // Arrange
            Parser parser = new Parser(7, 3);
            int col = 6;
            int row = 0;
            string expression = "not(mod(4 - 5^0; 7 - 3) = div(954;21) - 2*13)"; // Валідний математичний вираз

            // Act
            string result = parser.Calculated(col, row, expression);

            // Assert
            Assert.Equal("true", result); // Очікуємо результат true
        }
    }

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