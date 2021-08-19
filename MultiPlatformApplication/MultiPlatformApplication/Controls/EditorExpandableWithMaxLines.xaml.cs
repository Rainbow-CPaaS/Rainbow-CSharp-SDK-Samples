using MultiPlatformApplication.Helpers;
using Rainbow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditorExpandableWithMaxLines : ContentView
    {
        public event EventHandler<TextChangedEventArgs> TextChanged;

#region ValidationCommand Property

        public static readonly BindableProperty ValidationCommandProperty =
            BindableProperty.Create(nameof(ValidationCommand),
            typeof(ICommand),
            typeof(EditorExpandableWithMaxLines),
            defaultValue: null,
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: ValidationCommandPropertyProperyChanged);

        private static void ValidationCommandPropertyProperyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if( (bindable != null) && (newValue != null) )
            {
                EditorExpandableWithMaxLines editorExpandableWithMaxLines = (EditorExpandableWithMaxLines)bindable;
                MultiPlatformApplication.Effects.Entry.SetValidationCommand(editorExpandableWithMaxLines.Editor, (ICommand)newValue);
            }
        }

        public ICommand ValidationCommand
        {
            get
            {
                var obj = base.GetValue(ValidationCommandProperty);
                if (obj is ICommand)
                    return (ICommand)obj;
                return null;
            }
            set
            {
                base.SetValue(ValidationCommandProperty, value);
            }
        }

#endregion ValidationCommandProperty Property


#region ValidationKeyModifier Property

        public static readonly BindableProperty ValidationKeyModifierProperty =
            BindableProperty.Create(nameof(ValidationKeyModifier),
            typeof(String),
            typeof(EditorExpandableWithMaxLines),
            defaultValue: "",
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: ValidationKeyModifierProperyChanged);

        private static void ValidationKeyModifierProperyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if( (bindable != null) && (newValue != null) )
            {
                EditorExpandableWithMaxLines editorExpandableWithMaxLines = (EditorExpandableWithMaxLines)bindable;
                MultiPlatformApplication.Effects.Entry.SetValidationKeyModifier(editorExpandableWithMaxLines.Editor, (string)newValue);
            }
        }

        public String ValidationKeyModifier
        {
            get
            {
                var obj = base.GetValue(ValidationKeyModifierProperty);
                if (obj is String)
                    return (String)obj;
                return null;
            }
            set
            {
                base.SetValue(ValidationKeyModifierProperty, value);
            }
        }

#endregion ValidationKeyModifier Property


#region Text Property

        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(nameof(Text),
            typeof(String),
            typeof(EditorExpandableWithMaxLines),
            defaultValue: null,
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: TextProperyChanged);

        private static void TextProperyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((EditorExpandableWithMaxLines)bindable).Editor.Text = (String)newValue;
        }

        public String Text
        {
            get
            {
                var obj = base.GetValue(TextProperty);
                if (obj is String)
                    return (String)obj;
                return null;
            }
            set
            {
                base.SetValue(TextProperty, value);
            }
        }

#endregion Text Property

#region Placeholder Property

        public static readonly BindableProperty PlaceholderProperty =
            BindableProperty.Create(nameof(Placeholder),
            typeof(String),
            typeof(EditorExpandableWithMaxLines),
            defaultValue: null,
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: PlaceholderChanged);

        private static void PlaceholderChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((EditorExpandableWithMaxLines)bindable).Editor.Placeholder = (String)newValue;
        }

        public String Placeholder
        {
            get
            {
                var obj = base.GetValue(PlaceholderProperty);
                if (obj is String)
                    return (String)obj;
                return null;
            }
            set
            {
                base.SetValue(PlaceholderProperty, value);
            }
        }

#endregion Placeholder Property


#region MaxLines Property

        public static readonly BindableProperty MaxLinesProperty =
            BindableProperty.Create(nameof(MaxLines),
            typeof(int),
            typeof(EditorExpandableWithMaxLines),
            defaultValue: 5,
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: MaxLinesChanged);

        private static void MaxLinesChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((EditorExpandableWithMaxLines)bindable).MaxLines = (int)newValue;
        }

        public int MaxLines
        {
            get
            {
                var obj = base.GetValue(MaxLinesProperty);
                if (obj is int)
                    return (int)obj;
                return 5;
            }
            set
            {
                base.SetValue(MaxLinesProperty, value);
            }
        }

#endregion MaxLines Property

        double heigthOneLine = 0;
        double heigthTwoLines = 0;

        public EditorExpandableWithMaxLines()
        {
            InitializeComponent();

            Editor.PropertyChanged += Editor_PropertyChanged;
            Editor.TextChanged += Editor_TextChanged;
        }

        public String GetEditorText()
        {
            return Editor?.Text;
        }

        public void SetEditorText(String str)
        {
            Editor.Text = str;
        }

        public void SetFocus()
        {
            Editor.Focus();
        }

        private void Editor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Height")
            {
                if (heigthOneLine == 0)
                {
                    int countLines = CountLines();
                    if ((countLines == 1) && (Editor.Height != -1))
                        heigthOneLine = Editor.Height;
                }

                if (heigthTwoLines == 0)
                {
                    int countLines = CountLines();
                    if ((countLines == 2) && (Editor.Height != -1))
                        heigthTwoLines = Editor.Height;
                }
            }
        }

        private int CountLines()
        {
            int countLines = 0;

            if (Editor.Text != null)
            {
                if (Helper.IsWindowsPlatform())
                    countLines = Editor.Text.Count(f => f == '\r');
                else
                    countLines = Editor.Text.Count(f => f == '\n');
            }

            return countLines +1;
        }

        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            int countLines = CountLines();

            if (countLines >= MaxLines)
            {
                // Set HeightRequest to scrollview
                if (ScrollView.HeightRequest == -1)
                {
                    if( (heigthOneLine != 0) && (heigthTwoLines != 0) )
                        ScrollView.HeightRequest = heigthOneLine + (heigthTwoLines - heigthOneLine) * (MaxLines - 1);
                    else
                        ScrollView.HeightRequest = 80; // Need a backup value ... 
                }
            }
            else
            {
                // Remove HeightRequest to scrollview
                if (ScrollView.HeightRequest != -1)
                    ScrollView.HeightRequest = -1;
            }

            TextChanged?.Raise(sender, e);
        }
    }
}