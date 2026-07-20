using DVHAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace UMRO.DvhAnalysis.Script.Presentation.Controls
{
    /// <summary>
    /// Interaction logic for SMPCMetricInput.xaml
    /// </summary>
    public partial class SMPCMetricInput : Window
    {
        private Dictionary<string, double> _manualInput;

        public SMPCMetricInput(Dictionary<string, double> manualInput, string curStructureName, string curMetricName)
        {
            InitializeComponent();

            CurStructureName.Text = curStructureName;

            CurMetricName.Text = curMetricName;

            _manualInput = manualInput;
            
            PopulateInputFields();
        }

        private void PopulateInputFields()
        {
            foreach (var key in _manualInput.Keys)
            {
                var stackPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };

                var label = new Label {
                    Content = key, Width = 150, VerticalAlignment = VerticalAlignment.Center,
                    Style = (Style)FindResource("SectionHeader")
                };

                var textBox = new TextBox { Width = 200, Tag = key };

                stackPanel.Children.Add(label);
                stackPanel.Children.Add(textBox);

                InputPanel.Children.Add(stackPanel);
            }
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in InputPanel.Children)
            {
                if (child is StackPanel stackPanel)
                {
                    var textBox = stackPanel.Children[1] as TextBox;

                    var key = textBox.Tag.ToString();

                    if (double.TryParse(textBox.Text, out double value))
                    {
                        _manualInput[key] = value;
                    }
                    else if (string.IsNullOrEmpty(textBox.Text))
                    {
                        _manualInput[key] = double.NaN;
                    }
                    else
                    {
                        MessageBox.Show($"Invalid input for {key}. Please enter a valid number.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }

            DialogResult = true;
            Close();
        }
    }
}
