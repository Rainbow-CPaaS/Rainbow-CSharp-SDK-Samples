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
        public event EventHandler<FocusEventArgs> HasFocus;

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
                if(editorExpandableWithMaxLines.Editor != null)
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
                if (editorExpandableWithMaxLines.Editor != null)
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
            EditorExpandableWithMaxLines editorExpandableWithMaxLines = (EditorExpandableWithMaxLines)bindable;
            if (editorExpandableWithMaxLines.Editor != null)
                editorExpandableWithMaxLines.Editor.Text = (String)newValue;
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
            EditorExpandableWithMaxLines editorExpandableWithMaxLines = (EditorExpandableWithMaxLines)bindable;
            if (editorExpandableWithMaxLines.Editor != null)
                editorExpandableWithMaxLines.Editor.Placeholder = (String)newValue;
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
            EditorExpandableWithMaxLines editorExpandableWithMaxLines = (EditorExpandableWithMaxLines)bindable;
            editorExpandableWithMaxLines.MaxLines = (int)newValue;
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
                if(editorExpandableWithMaxLines.Editor != null)
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
        Editor Editor;
        ScrollView ScrollView;

        public EditorExpandableWithMaxLines()
        {
            InitializeComponent();

            Editor = CreateEditor();

            if (Helper.IsiOS())
            {
                ScrollView = null;
                Grid.Children.Add(Editor);
            }
            else
            {
                ScrollView = CreateScrollView();
                ScrollView.Content = Editor;
                Grid.Children.Add(ScrollView);
            }

            if (Editor != null)
            {
                Editor.TextChanged += Editor_TextChanged;
                Editor.Focused += Editor_Focused;
                Editor.Unfocused += Editor_Unfocused;
            }
            
        }

        private ScrollView CreateScrollView()
        {
            ScrollView scrollView = new ScrollView()
            {
                BackgroundColor = Color.Transparent,
                IsClippedToBounds = true,
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                HorizontalOptions = new LayoutOptions(LayoutAlignment.Fill, false),
                HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
                VerticalScrollBarVisibility = ScrollBarVisibility.Always
            };
            return scrollView;
        }

        private Editor CreateEditor()
        {
            Editor editor = new Editor()
            {
                Margin = new Thickness(0),
                AutoSize = EditorAutoSizeOption.TextChanges,
                FontSize = Helper.GetResourceDictionaryById<double>("FontSizeSmall"),
                BackgroundColor = Helper.GetResourceDictionaryById<Color>("ColorEntryBackground"),
                TextColor = Helper.GetResourceDictionaryById<Color>("ColorEntryText"),
                Placeholder = Helper.SdkWrapper.GetLabel("enterTextHere"),
                PlaceholderColor = Helper.GetResourceDictionaryById<Color>("ColorEntryPlaceHolder"),
                HorizontalOptions = new LayoutOptions(LayoutAlignment.Fill, false),
                VerticalOptions = new LayoutOptions(LayoutAlignment.Center, false)
            };
            return editor;
        }


        private void Editor_Unfocused(object sender, FocusEventArgs e)
        {
            HasFocus.Raise(this, new FocusEventArgs(this, false));
        }

        private void Editor_Focused(object sender, FocusEventArgs e)
        {
            HasFocus.Raise(this, new FocusEventArgs(this, true));
        }

        public String GetEditorText()
        {
            return Editor?.Text;
        }

        public void SetEditorText(String str)
        {
            if(Editor != null)
                Editor.Text = str;
        }

        public void SetFocus(bool focus = true)
        {
            if(focus)
                Editor?.Focus();
            else
                Editor?.Unfocus();
        }

        private void CheckHeight()
        {
            if ( (ScrollView != null) && (Editor != null) )
            {
                double maxH = GetMaxHeight();
                int countLines = CountLines();

                if ((Editor.Height > maxH) || (countLines >= MaxLines))
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

            if ( (Editor != null) && (Editor.Text != null) )
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