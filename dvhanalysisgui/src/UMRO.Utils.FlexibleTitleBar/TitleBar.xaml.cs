using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UMRO.Utils.FlexibleTitleBar
{
    public partial class TitleBar : UserControl
    {
        public static readonly DependencyProperty ShowClinicalWarningProperty = DependencyProperty.Register("ShowClinicalWarning", typeof(bool), typeof(TitleBar), new PropertyMetadata(true));

        public static readonly DependencyProperty UseSignatureLogoProperty = DependencyProperty.Register("UseSignatureLogo", typeof(bool), typeof(TitleBar), new PropertyMetadata(true));

        public static readonly DependencyProperty SignatureLogoProperty = DependencyProperty.Register("SignatureLogo", typeof(ImageSource), typeof(TitleBar), new PropertyMetadata(null));

        public static readonly DependencyProperty ProductNameProperty = DependencyProperty.Register("ProductName", typeof(string), typeof(TitleBar), new PropertyMetadata(""));

        public static readonly DependencyProperty ProductVersionProperty = DependencyProperty.Register("ProductVersion", typeof(string), typeof(TitleBar), new PropertyMetadata(""));

        public static readonly DependencyProperty PatientIdProperty = DependencyProperty.Register("PatientId", typeof(string), typeof(TitleBar), new PropertyMetadata(""));

        public static readonly DependencyProperty PatientLastNameProperty = DependencyProperty.Register("PatientLastName", typeof(string), typeof(TitleBar), new PropertyMetadata(""));

        public static readonly DependencyProperty PatientFirstNameProperty = DependencyProperty.Register("PatientFirstName", typeof(string), typeof(TitleBar), new PropertyMetadata(""));

        public static readonly DependencyProperty ShowCourseAndPlanProperty = DependencyProperty.Register("ShowCourseAndPlan", typeof(bool), typeof(TitleBar), new PropertyMetadata(true));

        public static readonly DependencyProperty CourseProperty = DependencyProperty.Register("Course", typeof(string), typeof(TitleBar), new PropertyMetadata(""));

        public static readonly DependencyProperty PlanProperty = DependencyProperty.Register("Plan", typeof(string), typeof(TitleBar), new PropertyMetadata(""));

        public static readonly DependencyProperty UserNameProperty = DependencyProperty.Register("UserName", typeof(string), typeof(TitleBar), new PropertyMetadata(""));

        public static readonly DependencyProperty HelpUriProperty = DependencyProperty.Register("HelpUri", typeof(string), typeof(TitleBar), new PropertyMetadata(""));

        public bool ShowClinicalWarning
        {
            get
            {
                return (bool)GetValue(ShowClinicalWarningProperty);
            }
            set
            {
                SetValue(ShowClinicalWarningProperty, value);
            }
        }

        public bool UseSignatureLogo
        {
            get
            {
                return (bool)GetValue(UseSignatureLogoProperty);
            }
            set
            {
                SetValue(UseSignatureLogoProperty, value);
            }
        }

        public ImageSource SignatureLogo
        {
            get
            {
                return (ImageSource)GetValue(SignatureLogoProperty);
            }
            set
            {
                SetValue(SignatureLogoProperty, value);
            }
        }

        public string ProductName
        {
            get
            {
                return (string)GetValue(ProductNameProperty);
            }
            set
            {
                SetValue(ProductNameProperty, value);
            }
        }

        public string ProductVersion
        {
            get
            {
                return (string)GetValue(ProductVersionProperty);
            }
            set
            {
                SetValue(ProductVersionProperty, value);
            }
        }

        public string PatientId
        {
            get
            {
                return (string)GetValue(PatientIdProperty);
            }
            set
            {
                SetValue(PatientIdProperty, value);
            }
        }

        public string PatientLastName
        {
            get
            {
                return (string)GetValue(PatientLastNameProperty);
            }
            set
            {
                SetValue(PatientLastNameProperty, value);
            }
        }

        public string PatientFirstName
        {
            get
            {
                return (string)GetValue(PatientFirstNameProperty);
            }
            set
            {
                SetValue(PatientFirstNameProperty, value);
            }
        }

        public bool ShowCourseAndPlan
        {
            get
            {
                return (bool)GetValue(ShowCourseAndPlanProperty);
            }
            set
            {
                SetValue(ShowCourseAndPlanProperty, value);
            }
        }

        public string Course
        {
            get
            {
                return (string)GetValue(CourseProperty);
            }
            set
            {
                SetValue(CourseProperty, value);
            }
        }

        public string Plan
        {
            get
            {
                return (string)GetValue(PlanProperty);
            }
            set
            {
                SetValue(PlanProperty, value);
            }
        }

        public string UserName
        {
            get
            {
                return (string)GetValue(UserNameProperty);
            }
            set
            {
                SetValue(UserNameProperty, value);
            }
        }

        public string HelpUri
        {
            get
            {
                return (string)GetValue(HelpUriProperty);
            }
            set
            {
                SetValue(HelpUriProperty, value);
            }
        }

        public TitleBar()
        {
            InitializeComponent();
        }

        private void OpenHelp(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(new Uri(HelpUri).AbsoluteUri));
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was a problem accessing help. Please contact the script's developer for help.\n\n" + ex.Message, "Help Error", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }
    }
}
