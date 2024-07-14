﻿using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Terminal.Gui;

/// <summary>
///     MessageBox displays a modal message to the user, with a title, a message and a series of options that the user
///     can choose from.
/// </summary>
/// <para>
///     The difference between the <see cref="Query(string, string, string[])"/> and
///     <see cref="ErrorQuery(string, string, string[])"/> method is the default set of colors used for the message box.
/// </para>
/// <para>
///     The following example pops up a <see cref="MessageBox"/> with the specified title and text, plus two
///     <see cref="Button"/>s. The value -1 is returned when the user cancels the <see cref="MessageBox"/> by pressing the
///     ESC key.
/// </para>
/// <example>
///     <code lang="c#">
/// var n = MessageBox.Query ("Quit Demo", "Are you sure you want to quit this demo?", "Yes", "No");
/// if (n == 0)
///    quit = true;
/// else
///    quit = false;
/// </code>
/// </example>
public static class MessageBox
{
    /// <summary>
    ///     Defines the default border styling for <see cref="MessageBox"/>. Can be configured via
    ///     <see cref="ConfigurationManager"/>.
    /// </summary>
    [SerializableConfigurationProperty (Scope = typeof (ThemeScope))]
    [JsonConverter (typeof (JsonStringEnumConverter<LineStyle>))]
    public static LineStyle DefaultBorderStyle { get; set; } = LineStyle.Single;

    /// <summary>
    ///     Defines the default minimum MessageBox width, as a percentage of the container width. Can be configured via
    ///     <see cref="ConfigurationManager"/>.
    /// </summary>
    [SerializableConfigurationProperty (Scope = typeof (ThemeScope))]
    public static int DefaultMinimumWidth { get; set; } = 60;

    /// <summary>
    ///     Defines the default minimum Dialog height, as a percentage of the container width. Can be configured via
    ///     <see cref="ConfigurationManager"/>.
    /// </summary>
    [SerializableConfigurationProperty (Scope = typeof (ThemeScope))]
    public static int DefaultMinimumHeight { get; set; } = 5;
    /// <summary>
    ///     The index of the selected button, or -1 if the user pressed <see cref="Application.QuitKey"/> to close the MessageBox. This is useful for web
    ///     based console where there is no SynchronizationContext or TaskScheduler.
    /// </summary>
    /// <remarks>
    ///     Warning: This is a global variable and should be used with caution. It is not thread safe.
    /// </remarks>
    public static int Clicked { get; private set; } = -1;

    /// <summary>
    ///     Presents an error <see cref="MessageBox"/> with the specified title and message and a list of buttons.
    /// </summary>
    /// <returns>The index of the selected button, or -1 if the user pressed <see cref="Application.QuitKey"/> to close the MessageBox.</returns>
    /// <param name="width">Width for the MessageBox.</param>
    /// <param name="height">Height for the MessageBox.</param>
    /// <param name="title">Title for the MessageBox.</param>
    /// <param name="message">Message to display; might contain multiple lines. The message will be word=wrapped by default.</param>
    /// <param name="buttons">Array of buttons to add.</param>
    /// <remarks>
    ///     Use <see cref="ErrorQuery(string, string, string[])"/> instead; it automatically sizes the MessageBox based on
    ///     the contents.
    /// </remarks>
    public static int ErrorQuery (int width, int height, string title, string message, params string [] buttons)
    {
        return QueryFull (true, width, height, title, message, 0, true, buttons);
    }

    /// <summary>
    ///     Presents an error <see cref="MessageBox"/> with the specified title and message and a list of buttons to show
    ///     to the user.
    /// </summary>
    /// <returns>The index of the selected button, or -1 if the user pressed <see cref="Application.QuitKey"/> to close the MessageBox.</returns>
    /// <param name="title">Title for the query.</param>
    /// <param name="message">Message to display; might contain multiple lines. The message will be word=wrapped by default.</param>
    /// <param name="buttons">Array of buttons to add.</param>
    /// <remarks>
    ///     The message box will be vertically and horizontally centered in the container and the size will be
    ///     automatically determined from the size of the title, message. and buttons.
    /// </remarks>
    public static int ErrorQuery (string title, string message, params string [] buttons) { return QueryFull (true, 0, 0, title, message, 0, true, buttons); }

    /// <summary>
    ///     Presents an error <see cref="MessageBox"/> with the specified title and message and a list of buttons.
    /// </summary>
    /// <returns>The index of the selected button, or -1 if the user pressed <see cref="Application.QuitKey"/> to close the MessageBox.</returns>
    /// <param name="width">Width for the MessageBox.</param>
    /// <param name="height">Height for the MessageBox.</param>
    /// <param name="title">Title for the MessageBox.</param>
    /// <param name="message">Message to display; might contain multiple lines. The message will be word=wrapped by default.</param>
    /// <param name="defaultButton">Index of the default button.</param>
    /// <param name="buttons">Array of buttons to add.</param>
    /// <remarks>
    ///     Use <see cref="ErrorQuery(string, string, string[])"/> instead; it automatically sizes the MessageBox based on
    ///     the contents.
    /// </remarks>
    public static int ErrorQuery (
        int width,
        int height,
        string title,
        string message,
        int defaultButton = 0,
        params string [] buttons
    )
    {
        return QueryFull (true, width, height, title, message, defaultButton, true, buttons);
    }

    /// <summary>
    ///     Presents an error <see cref="MessageBox"/> with the specified title and message and a list of buttons to show
    ///     to the user.
    /// </summary>
    /// <returns>The index of the selected button, or -1 if the user pressed <see cref="Application.QuitKey"/> to close the MessageBox.</returns>
    /// <param name="title">Title for the MessageBox.</param>
    /// <param name="message">Message to display; might contain multiple lines. The message will be word=wrapped by default.</param>
    /// <param name="defaultButton">Index of the default button.</param>
    /// <param name="buttons">Array of buttons to add.</param>
    /// <remarks>
    ///     The message box will be vertically and horizontally centered in the container and the size will be
    ///     automatically determined from the size of the title, message. and buttons.
    /// </remarks>
    public static int ErrorQuery (string title, string message, int defaultButton = 0, params string [] buttons)
    {
        return QueryFull (true, 0, 0, title, message, defaultButton, true, buttons);
    }

    /// <summary>
    ///     Presents an error <see cref="MessageBox"/> with the specified title and message and a list of buttons to show
    ///     to the user.
    /// </summary>
    /// <returns>The index of the selected button, or -1 if the user pressed <see cref="Application.QuitKey"/> to close the MessageBox.</returns>
    /// <param name="width">Width for the window.</param>
    /// <param name="height">Height for the window.</param>
    /// <param name="title">Title for the query.</param>
    /// <param name="message">Message to display; might contain multiple lines. The message will be word=wrapped by default.</param>
    /// <param name="defaultButton">Index of the default button.</param>
    /// <param name="wrapMessage">If wrap the message or not.</param>
    /// <param name="buttons">Array of buttons to add.</param>
    /// <remarks>
    ///     Use <see cref="ErrorQuery(string, string, string[])"/> instead; it automatically sizes the MessageBox based on
    ///     the contents.
    /// </remarks>
    public static int ErrorQuery (
        int width,
        int height,
        string title,
        string message,
        int defaultButton = 0,
        bool wrapMessage = true,
        params string [] buttons
    )
    {
        return QueryFull (true, width, height, title, message, defaultButton, wrapMessage, buttons);
    }

    /// <summary>
    ///     Presents an error <see cref="MessageBox"/> with the specified title and message and a list of buttons to show
    ///     to the user.
    /// </summary>
    /// <returns>The index of the selected button, or -1 if the user pressed <see cref="Application.QuitKey"/> to close the MessageBox.</returns>
    /// <param name="title">Title for the query.</param>
    /// <param name="message">Message to display; might contain multiple lines. The message will be word=wrapped by default.</param>
    /// <param name="defaultButton">Index of the default button.</param>
    /// <param name="wrapMessage">If wrap the message or not. The default is <see langword="true"/></param>
    /// <param name="buttons">Array of buttons to add.</param>
    /// <remarks>
    ///     The message box will be vertically and horizontally centered in the container and the size will be
    ///     automatically determined from the size of the title, message. and buttons.
    /// </remarks>
    public static int ErrorQuery (
        string title,
        string message,
        int defaultButton = 0,
        bool wrapMessage = true,
        params string [] buttons
    )
    {
        return QueryFull (true, 0, 0, title, message, defaultButton, wrapMessage, buttons);
    }

    /// <summary>
    ///     Presents a <see cref="MessageBox"/> with the specified title and message and a list of buttons.
    /// </summary>
    /// <returns>The index of the selected button, or -1 if the user pressed <see cref="Application.QuitKey"/> to close the MessageBox.</returns>
    /// <param name="width">Width for the MessageBox.</param>
    /// <param name="height">Height for the MessageBox.</param>
    /// <param name="title">Title for the MessageBox.</param>
    /// <param name="message">Message to display; might contain multiple lines. The message will be word=wrapped by default.</param>
    /// <param name="buttons">Array of buttons to add.</param>
    /// <remarks>
    ///     Use <see cref="Query(string, string, string[])"/> instead; it automatically sizes the MessageBox based on
    ///     the contents.
    /// </remarks>
    public static int Query (int width, int height, string title, string message, params string [] buttons)
    {
        return QueryFull (false, width, height, title, message, 0, true, buttons);
    }

    /// <summary>
    ///     Presents a <see cref="MessageBox"/> with the specified title and message and a list of buttons.
    /// </summary>
    /// <returns>The index of the selected button, or -1 if the user pressed <see cref="Application.QuitKey"/> to close the MessageBox.</returns>
    /// <param name="title">Title for the MessageBox.</param>
    /// <param name="message">Message to display; might contain multiple lines. The message will be word=wrapped by default.</param>
    /// <param name="buttons">Array of buttons to add.</param>
    /// <remarks>
    /// <para>
    ///     The message box will be vertically and horizontally centered in the container and the size will be
    ///     automatically determined from the size of the title, message. and buttons.
    /// </para>
    /// <para>
    ///     Use <see cref="Query(string, string, string[])"/> instead; it automatically sizes the MessageBox based on
    ///     the contents.
    /// </para>
    /// </remarks>
    public static int Query (string title, string message, params string [] buttons) { return QueryFull (false, 0, 0, title, message, 0, true, buttons); }

    /// <summary>
    ///     Presents a <see cref="MessageBox"/> with the specified title and message and a list of buttons.
    /// </summary>
    /// <returns>The index of the selected button, or -1 if the user pressed <see cref="Application.QuitKey"/> to close the MessageBox.</returns>
    /// <param name="width">Width for the window.</param>
    /// <param name="height">Height for the window.</param>
    /// <param name="title">Title for the MessageBox.</param>
    /// <param name="message">Message to display; might contain multiple lines. The message will be word=wrapped by default.</param>
    /// <param name="defaultButton">Index of the default button.</param>
    /// <param name="buttons">Array of buttons to add.</param>
    /// <remarks>
    /// <para>
    ///     The message box will be vertically and horizontally centered in the container and the size will be
    ///     automatically determined from the size of the title, message. and buttons.
    /// </para>
    /// <para>
    ///     Use <see cref="Query(string, string, string[])"/> instead; it automatically sizes the MessageBox based on
    ///     the contents.
    /// </para>
    /// </remarks>
    public static int Query (
        int width,
        int height,
        string title,
        string message,
        int defaultButton = 0,
        params string [] buttons
    )
    {
        return QueryFull (false, width, height, title, message, defaultButton, true, buttons);
    }

    /// <summary>
    ///     Presents a <see cref="MessageBox"/> with the specified title and message and a list of buttons.
    /// </summary>
    /// <returns>The index of the selected button, or -1 if the user pressed <see cref="Application.QuitKey"/> to close the MessageBox.</returns>
    /// <param name="title">Title for the MessageBox.</param>
    /// <param name="message">Message to display; might contain multiple lines. The message will be word=wrapped by default.</param>
    /// <param name="defaultButton">Index of the default button.</param>
    /// <param name="buttons">Array of buttons to add.</param>
    /// <remarks>
    ///     The message box will be vertically and horizontally centered in the container and the size will be
    ///     automatically determined from the size of the message and buttons.
    /// </remarks>
    public static int Query (string title, string message, int defaultButton = 0, params string [] buttons)
    {
        return QueryFull (false, 0, 0, title, message, defaultButton, true, buttons);
    }

    /// <summary>
    ///     Presents a <see cref="MessageBox"/> with the specified title and message and a list of buttons to show
    ///     to the user.
    /// </summary>
    /// <returns>The index of the selected button, or -1 if the user pressed <see cref="Application.QuitKey"/> to close the MessageBox.</returns>
    /// <param name="width">Width for the window.</param>
    /// <param name="height">Height for the window.</param>
    /// <param name="title">Title for the query.</param>
    /// <param name="message">Message to display, might contain multiple lines.</param>
    /// <param name="defaultButton">Index of the default button.</param>
    /// <param name="wrapMessage">If wrap the message or not.</param>
    /// <param name="buttons">Array of buttons to add.</param>
    /// <remarks>
    ///     Use <see cref="Query(string, string, string[])"/> instead; it automatically sizes the MessageBox based on the
    ///     contents.
    /// </remarks>
    public static int Query (
        int width,
        int height,
        string title,
        string message,
        int defaultButton = 0,
        bool wrapMessage = true,
        params string [] buttons
    )
    {
        return QueryFull (false, width, height, title, message, defaultButton, wrapMessage, buttons);
    }

    /// <summary>
    ///     Presents a <see cref="MessageBox"/> with the specified title and message and a list of buttons to show
    ///     to the user.
    /// </summary>
    /// <returns>The index of the selected button, or -1 if the user pressed <see cref="Application.QuitKey"/> to close the MessageBox.</returns>
    /// <param name="title">Title for the query.</param>
    /// <param name="message">Message to display, might contain multiple lines.</param>
    /// <param name="defaultButton">Index of the default button.</param>
    /// <param name="wrapMessage">If wrap the message or not.</param>
    /// <param name="buttons">Array of buttons to add.</param>
    public static int Query (
        string title,
        string message,
        int defaultButton = 0,
        bool wrapMessage = true,
        params string [] buttons
    )
    {
        return QueryFull (false, 0, 0, title, message, defaultButton, wrapMessage, buttons);
    }

    private static int QueryFull (
        bool useErrorColors,
        int width,
        int height,
        string title,
        string message,
        int defaultButton = 0,
        bool wrapMessage = true,
        params string [] buttons
    )
    {
        // Create button array for Dialog
        var count = 0;
        List<Button> buttonList = new ();

        if (buttons is { })
        {
            if (defaultButton > buttons.Length - 1)
            {
                defaultButton = buttons.Length - 1;
            }

            foreach (string s in buttons)
            {
                var b = new Button
                {
                    Text = s,
                };

                if (count == defaultButton)
                {
                    b.IsDefault = true;
                }

                buttonList.Add (b);
                count++;
            }
        }

        var d = new Dialog
        {
            Title = title,
            Buttons = buttonList.ToArray (),
            ButtonAlignment = Alignment.Center,
            ButtonAlignmentModes = AlignmentModes.StartToEnd | AlignmentModes.AddSpaceBetweenItems,
            BorderStyle = MessageBox.DefaultBorderStyle,
            Width = Dim.Auto (DimAutoStyle.Auto, minimumContentDim: 1, maximumContentDim: Dim.Percent (90)),
            Height = Dim.Auto (DimAutoStyle.Auto, minimumContentDim: 2, maximumContentDim: Dim.Percent (90)),
        };

        if (width != 0)
        {
            d.Width = width;
        }

        if (height != 0)
        {
            d.Height = height;
        }

        d.ColorScheme = useErrorColors ? Colors.ColorSchemes ["Error"] : Colors.ColorSchemes ["Dialog"];

        d.LayoutComplete += (s, e) =>
        {
            if (wrapMessage)
            {
                int buttonHeight = buttonList.Count > 0 ? buttonList [0].Frame.Height : 0;
                Debug.Assert (d.TextFormatter.WordWrap);
                d.TextFormatter.Size = new Size (d.GetContentSize ().Width, Application.Driver.Screen.Height);
                Size textSize = d.TextFormatter.GetAutoSize ();
                textSize.Height += buttonHeight;

                if (textSize != d.TextFormatter.Size)
                {
                    //d.TextFormatter.Size = textSize;
                    //d.SetContentSize (textSize);
                    d.SetNeedsLayout ();
                    //d.SetRelativeLayout (Application.Driver.Screen.Size);
                }
            }
        };

        d.HotKeySpecifier = new Rune ('\xFFFF');
        d.Text = message;
        d.TextAlignment = Alignment.Center;
        d.VerticalTextAlignment = Alignment.Start;
        d.TextFormatter.WordWrap = wrapMessage;
        d.TextFormatter.MultiLine = !wrapMessage;

        // Add two lines to push buttons down two rows
        // BUGBUG: The " " are here due to a bug in TextFormater.Format that strips trailing newlines when .Wordwrap = true
       // d.Text += Environment.NewLine + " " + Environment.NewLine + " ";

        d.ColorScheme = new ColorScheme (d.ColorScheme)
        {
            Focus = d.ColorScheme.Normal
        };

        // Setup actions
        Clicked = -1;

        for (var n = 0; n < buttonList.Count; n++)
        {
            int buttonId = n;
            Button b = buttonList [n];

            b.Accept += (s, e) =>
                         {
                             Clicked = buttonId;
                             Application.RequestStop ();
                         };

            if (b.IsDefault)
            {
                b.SetFocus ();
            }
        }

        // Run the modal; do not shutdown the mainloop driver when done
        Application.Run (d);
        d.Dispose ();

        return Clicked;

    }
}
