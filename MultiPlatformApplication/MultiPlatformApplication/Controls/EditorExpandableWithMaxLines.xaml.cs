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

#region BreakLineModifier Property

        public static readonly BindableProperty BreakLineModifierProperty =
            BindableProperty.Create(nameof(BreakLineModifier),
            typeof(String),
            typeof(EditorExpandableWithMaxLines),
            defaultValue: "",
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: BreakLineModifierProperyChanged);

        private static void BreakLineModifierProperyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if( (bindable != null) && (newValue != null) )
            {
                EditorExpandableWithMaxLines editorExpandableWithMaxLines = (EditorExpandableWithMaxLines)bindable;
                MultiPlatformApplication.Effects.Entry.SetBreakLineModifier(editorExpandableWithMaxLines.Editor, (string)newValue);
            }
        }

        public String BreakLineModifier
        {
            get
            {
                var obj = base.GetValue(BreakLineModifierProperty);
                if (obj is String)
                    return (String)obj;
                return null;
            }
            set
            {
                base.SetValue(BreakLineModifierProperty, value);
            }
        }

#endregion BreakLineModifier Property

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

#region MinimumWidth Property

        public static readonly BindableProperty MinimumWidthProperty =
            BindableProperty.Create(nameof(MinimumWidth),
            typeof(double),
            typeof(EditorExpandableWithMaxLines),
            defaultValue: (double)-1,
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: MinimumWidthChanged);

        private static void MinimumWidthChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if ((bindable != null) && (newValue != null))
            {
                EditorExpandableWithMaxLines editorExpandableWithMaxLines = (EditorExpandableWithMaxLines)bindable;
                MultiPlatformApplication.Effects.Entry.SetMinimumWidth(editorExpandableWithMaxLines.Editor, (double)newValue);
            }
        }

        public double MinimumWidth
        {
            get
            {
                var obj = base.GetValue(MinimumWidthProperty);
                if (obj is int)
                    return (int)obj;
                return 5;
            }
            set
            {
                base.SetValue(MinimumWidthProperty, value);
            }
        }

#endregion MinimumWidth Property


        double heightOneLine = 32;
        double heightTwoLines = 45;
        double maxHeight;

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

        private void CheckHeight()
        {
            double maxH = GetMaxHeight();
            int countLines = CountLines();

            if ( (Editor.Height > maxH) || (countLines >= MaxLines))
            {
                // Set HeightRequest to scrollview
                if (ScrollView.HeightRequest == -1)
                    ScrollView.HeightRequest = maxH;
            }
            else
            {
                // Remove HeightRequest to scrollview
                if (ScrollView.HeightRequest != -1)
                    ScrollView.HeightRequest = -1;
            }
        }

        private double GetMaxHeight()
        {
            if(maxHeight == 0)
            {
                maxHeight = heightOneLine + (heightTwoLines - heightOneLine) * (MaxLines - 1);
            }
            return maxHeight;
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
            CheckHeight();

            TextChanged?.Raise(sender, e);
        }

        private void Editor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Height")
                CheckHeight();
        }
    }
}