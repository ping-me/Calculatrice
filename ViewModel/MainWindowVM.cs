using Calculatrice.Model;
using System.ComponentModel;

namespace Calculatrice.ViewModel
{
    public class MainWindowVM : INotifyPropertyChanged
    {
        public MainWindowVM()
        {
            display = "0";
            expressionDisplay = string.Empty;
            openedParenthesis = 0;
            evaluator = new Evaluator();

            NumberKeyCommand = new NumberCommand(this);
            OperatorKeyCommand = new OperatorCommand(this);
            OtherKeyCommand = new OtherCommand(this);
        }

        private string display;
        private string expressionDisplay;
        private int openedParenthesis;
        private readonly Evaluator evaluator;

        public NumberCommand NumberKeyCommand { get; set; }
        public OperatorCommand OperatorKeyCommand { get; set; }
        public OtherCommand OtherKeyCommand { get; set; }

        public string Display
        { 
            get { return display; }
            set
            {
                display = value;
                OnPropertyChanged("Display");
            }
        }

        public string ExpressionDisplay
        {
            get { return expressionDisplay; }
            set
            {
                expressionDisplay = value;
                OnPropertyChanged("ExpressionDisplay");
            }
        }

        public void ClearDisplay()
        {
            Display = "0";
        }

        #region Can Execute checks

        public bool CanExecuteNumberCommand(string parameter)
        {
            switch (parameter)
            {
                case ".":
                    if (evaluator.GetTokenCount() != 0)
                    {
                        if (Display.Contains(","))
                        {
                            return false;
                        }
                        else
                        {
                            return (evaluator.GetLastTokenType() != Evaluator.TokenType.Operator);
                        }
                    }
                    else
                    {
                        return false;
                    }
                case "±":
                    return ((double.Parse(Display) != 0) && (evaluator.GetTokenCount() != 0) && (evaluator.GetLastTokenType() != Evaluator.TokenType.Operator));
                default:
                    if (evaluator.GetTokenCount() != 0)
                    {
                        return (evaluator.GetLastTokenContent() != ")");
                    }
                    else
                    {
                        return true;
                    }
            }
        }

        public bool CanExecuteOperatorCommand(string parameter)
        {
            if (evaluator.GetTokenCount() != 0)
            {
                switch (parameter)
                {
                    case "(":
                        return ((evaluator.GetLastTokenType() == Evaluator.TokenType.Operator) && (evaluator.GetLastTokenContent() != ")"));
                    case ")":
                        if (openedParenthesis > 0)
                        {
                            return ((evaluator.GetLastTokenType() == Evaluator.TokenType.Operand) || (evaluator.GetLastTokenContent() == ")"));
                        }
                        else
                        {
                            return false;
                        }
                    default:
                        return (evaluator.GetLastTokenContent() != "(");
                }
            }
            else
            {
                return (parameter == "(");
            }
        }

        public bool CanExecuteOtherCommand(string parameter)
        {
            switch (parameter)
            {
                case "=":
                    return ((openedParenthesis == 0) && (evaluator.GetTokenCount() > 2) && ((evaluator.GetLastTokenType() != Evaluator.TokenType.Operator) || (evaluator.GetLastTokenContent() == ")")));
                case "C":
                    return (evaluator.GetTokenCount() > 0);
                default:
                    return true;
            }
        }

        #endregion

        #region Execute Command fonctions

        public void ExecuteNumberCommand(string parameter)
        {
            // If no tokens, adds the first one and sets it to 0
            if (evaluator.GetTokenCount() == 0)
            {
                evaluator.AddToken(Evaluator.TokenType.Operand, "0");
                ClearDisplay();
            }

            // Creates operand if last token is an operator
            if (evaluator.GetLastTokenType() == Evaluator.TokenType.Operator)
            {
                if ((parameter != ".") && (parameter != "±"))
                {
                    evaluator.AddToken(Evaluator.TokenType.Operand, parameter);
                    ClearDisplay();
                }
                else
                {
                    return;
                }
            }

            switch (parameter)
            {
                // Displays a decimal
                case ".":
                    if (!Display.Contains(","))
                    {
                        Display += ",";
                    }
                    break;
                // Invert sign
                case "±":
                    if (Display.Contains("-"))
                    {
                        Display = Display.TrimStart('-');
                    }
                    else
                    {
                        Display = $"-{Display}";
                    }
                    break;
                // Display/Update current operand
                default:
                    if (Display != "0")
                    {
                        Display += parameter;
                    }
                    else
                    {
                        Display = parameter;
                    }
                    break;
            }
            // Updates expression
            evaluator.UpdateLastToken(Evaluator.TokenType.Operand, Display);
            ExpressionDisplay = evaluator.GetExpression();
        }

        public void ExecuteOperatorCommand(string parameter)
        {
            // If no tokens, disable all except (
            if (evaluator.GetTokenCount() != 0)
            {
                switch (parameter)
                {
                    case "(":
                        evaluator.AddToken(Evaluator.TokenType.Operator, "(");
                        openedParenthesis++;
                        break;
                    case ")":
                        evaluator.AddToken(Evaluator.TokenType.Operator, ")");
                        openedParenthesis--;
                        break;
                    default:
                        if (evaluator.GetLastTokenType() == Evaluator.TokenType.Operand)
                        {
                            // Adds operator to expression
                            evaluator.AddToken(Evaluator.TokenType.Operator, parameter);
                        }
                        else
                        {
                            if (evaluator.GetLastTokenContent() == ")")
                            {
                                evaluator.AddToken(Evaluator.TokenType.Operator, parameter);
                            }
                            else
                            {
                                // Updates if operator already entered
                                evaluator.UpdateLastToken(Evaluator.TokenType.Operator, parameter);
                                break;
                            }
                        }
                        break;
                }
            }
            else if (parameter == "(")
            {
                // Expression starting with parenthesis
                evaluator.AddToken(Evaluator.TokenType.Operator, parameter);
                Display = "0";
                openedParenthesis++;
            }
            ExpressionDisplay = evaluator.GetExpression();
        }

        public void ExecuteOtherCommand(string parameter)
        {
            switch (parameter)
            {
                case "=":
                    ExpressionDisplay = $"{evaluator.GetExpression()}=";
                    Display = evaluator.Resolve();
                    evaluator.ClearTokens();
                    break;
                case "C":
                    ClearDisplay();
                    if (evaluator.GetLastTokenContent() == ")")
                    {
                        openedParenthesis++;
                    }
                    if (evaluator.GetLastTokenContent() == "(")
                    {
                        openedParenthesis--;
                    }
                    evaluator.RemoveLastToken();
                    if (evaluator.GetTokenCount() > 0)
                    {
                        if (evaluator.GetLastTokenType() == Evaluator.TokenType.Operand)
                        {
                            Display = evaluator.GetLastTokenContent();
                        }
                    }
                    break;
            }
            if (parameter != "=")
            {
                ExpressionDisplay = evaluator.GetExpression();
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string parameter)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(parameter));
        }
    }
}
