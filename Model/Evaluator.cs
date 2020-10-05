using System.Collections.Generic;

namespace Calculatrice.Model
{
    public class Evaluator
    {
        public Evaluator()
        {
            tokens = new List<Token>();
        }

        public enum TokenType
        {
            Operator,
            Operand
        }

        private readonly List<Token> tokens;

        private class Token
        {
            public TokenType tokenType;
            public string tokenContent;
            public Token(TokenType type, string content)
            {
                tokenType = type;
                tokenContent = content;
            }
        }

        public string GetExpression()
        {
            string expression = string.Empty;
            foreach (Token t in tokens)
            {
                if (t.tokenType == TokenType.Operand)
                {
                    if (double.Parse(t.tokenContent) < 0)
                    {
                        expression += $"({t.tokenContent})";
                        continue;
                    }
                }
                expression += t.tokenContent;
            }
            return expression;
        }

        public void AddToken(TokenType type, string content)
        {
            tokens.Add(new Token(type, content));
        }

        public void UpdateLastToken(TokenType type, string content)
        {
            RemoveLastToken();
            AddToken(type, content);
        }

        public int GetTokenCount()
        {
            return tokens.Count;
        }

        public TokenType GetLastTokenType()
        {
            return tokens[tokens.Count - 1].tokenType;
        }

        public string GetLastTokenContent()
        {
            return tokens[tokens.Count - 1].tokenContent;
        }

        public void RemoveLastToken()
        {
            tokens.RemoveAt(tokens.Count - 1);
        }

        public void ClearTokens()
        {
            tokens.Clear();
        }

        public string Resolve()
        {
            return Solve(tokens);
        }

        private void CollapseTokens(List<Token> t, int index, int tokenCount, string content)
        {
            for (int repeat = 0; repeat < tokenCount; repeat++)
            {
                t.RemoveAt(index);
            }
            t.Insert(index, new Token(TokenType.Operand, content));
        }

        private string Solve(List<Token> t)
        {
            Token currentToken;
            int currentIndex;
            Stack<int> parenthesisIndex = new Stack<int>();

            // Parenthesis resolve
            currentIndex = 0;
            do
            {
                currentToken = t[currentIndex];

                if (currentToken.tokenType == TokenType.Operator)
                {
                    if (currentToken.tokenContent == "(")
                    {
                        parenthesisIndex.Push(currentIndex++);
                        continue;
                    }
                    else if (currentToken.tokenContent == ")")
                    {
                        int subExpStartIndex = parenthesisIndex.Pop();
                        int subExpLength = currentIndex - subExpStartIndex - 1;
                        // Sub expression solved and collapsed to an operand when finding a closed parenthesis
                        CollapseTokens(t, subExpStartIndex, subExpLength + 2, Solve(t.GetRange(subExpStartIndex + 1, subExpLength)));
                        currentIndex = subExpStartIndex;
                        continue;
                    }
                }
                currentIndex++;
            } while (currentIndex < t.Count);

            if (t.Count > 1)
            {
                // Mul/Div resolve
                currentIndex = 1;
                do
                {
                    currentToken = t[currentIndex];

                    if (currentToken.tokenType == TokenType.Operand)
                    {
                        currentIndex++;
                        continue;
                    }
                    if (currentToken.tokenType == TokenType.Operator)
                    {
                        if (currentToken.tokenContent == "×")
                        {
                            // Multiply
                            currentIndex--;
                            CollapseTokens(t, currentIndex++, 3, (double.Parse(t[currentIndex - 1].tokenContent) * double.Parse(t[currentIndex + 1].tokenContent)).ToString());
                        }
                        else if (currentToken.tokenContent == "÷")
                        {
                            // Divide
                            currentIndex--;
                            CollapseTokens(t, currentIndex++, 3, (double.Parse(t[currentIndex - 1].tokenContent) / double.Parse(t[currentIndex + 1].tokenContent)).ToString());
                        }
                        else
                        {
                            // Addition or substraction, skipping next 2 tokens
                            currentIndex += 2;
                        }
                    }
                } while (currentIndex < t.Count);
            }

            if (t.Count > 1)
            {
                // Add/Sub resolve
                currentIndex = 1;
                do
                {
                    currentToken = t[currentIndex];

                    if (currentToken.tokenType == TokenType.Operand)
                    {
                        currentIndex++;
                        continue;
                    }
                    if (currentToken.tokenType == TokenType.Operator)
                    {
                        if (currentToken.tokenContent == "+")
                        {
                            // Add
                            currentIndex--;
                            CollapseTokens(t, currentIndex++, 3, (double.Parse(t[currentIndex - 1].tokenContent) + double.Parse(t[currentIndex + 1].tokenContent)).ToString());
                        }
                        else if (currentToken.tokenContent == "-")
                        {
                            // Substract
                            currentIndex--;
                            CollapseTokens(t, currentIndex++, 3, (double.Parse(t[currentIndex - 1].tokenContent) - double.Parse(t[currentIndex + 1].tokenContent)).ToString());
                        }
                    }
                } while (currentIndex < t.Count);
            }

            // Last token left is the result
            return t[0].tokenContent;
        }
    }
}
