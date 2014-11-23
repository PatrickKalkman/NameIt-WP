// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

// This control is based on the TransitioningBackgroundControl from the 
// Silverlight Toolkit. We'll see how this goes! - Jeff

using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Globalization;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls.Primitives;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a control with a single piece of content and when that content 
    /// changes performs a transition animation. 
    /// </summary>
    /// <QualityBand>Experimental</QualityBand>
    /// <remarks>The API for this control will change considerably in the future.</remarks>
    [TemplateVisualState(GroupName = PresentationGroup, Name = NormalState)]
    [TemplateVisualState(GroupName = PresentationGroup, Name = DefaultTransitionState)]
    [TemplatePart(Name = PreviousContentPresentationSitePartName, Type = typeof(Grid))]
    [TemplatePart(Name = CurrentContentPresentationSitePartName, Type = typeof(Grid))]
    public class TransitioningBackgroundControl : Control
    {
        #region Visual state names
        /// <summary>
        /// The name of the group that holds the presentation states.
        /// </summary>
        private const string PresentationGroup = "PresentationStates";

        /// <summary>
        /// The name of the state that represents a normal situation where no
        /// transition is currently being used.
        /// </summary>
        private const string NormalState = "Normal";

        /// <summary>
        /// The name of the state that represents the default transition.
        /// </summary>
        public const string DefaultTransitionState = "DefaultTransition";
        #endregion Visual state names

        #region Template part names
        /// <summary>
        /// The name of the control that will display the previous content.
        /// </summary>
        internal const string PreviousContentPresentationSitePartName = "PreviousContentPresentationSite";

        /// <summary>
        /// The name of the control that will display the current content.
        /// </summary>
        internal const string CurrentContentPresentationSitePartName = "CurrentContentPresentationSite";

        #endregion Template part names

        /// <summary>
        /// The panning layer so that we can hook into the update mechanism.
        /// </summary>
        private UpdatingPanningLayer _updatingPanningLayer;

        #region TemplateParts
        /// <summary>
        /// Gets or sets the current content presentation site.
        /// </summary>
        /// <value>The current content presentation site.</value>
        private Grid CurrentContentPresentationSite { get; set; }

        /// <summary>
        /// Gets or sets the previous content presentation site.
        /// </summary>
        /// <value>The previous content presentation site.</value>
        private Grid PreviousContentPresentationSite { get; set; } 
        #endregion TemplateParts

        #region public bool IsTransitioning

        /// <summary>
        /// Indicates whether the control allows writing IsTransitioning.
        /// </summary>
        private bool _allowIsTransitioningWrite;

        /// <summary>
        /// Gets a value indicating whether this instance is currently performing
        /// a transition.
        /// </summary>
        public bool IsTransitioning
        {
            get { return (bool)GetValue(IsTransitioningProperty); }
            private set
            {
                _allowIsTransitioningWrite = true;
                SetValue(IsTransitioningProperty, value);
                _allowIsTransitioningWrite = false;
            }
        }

        /// <summary>
        /// Identifies the IsTransitioning dependency property.
        /// </summary>
        public static readonly DependencyProperty IsTransitioningProperty =
            DependencyProperty.Register(
                "IsTransitioning",
                typeof(bool),
                typeof(TransitioningBackgroundControl),
                new PropertyMetadata(OnIsTransitioningPropertyChanged));

        /// <summary>
        /// IsTransitioningProperty property changed handler.
        /// </summary>
        /// <param name="d">TransitioningBackgroundControl that changed its IsTransitioning.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnIsTransitioningPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TransitioningBackgroundControl source = (TransitioningBackgroundControl)d;

            if (!source._allowIsTransitioningWrite)
            {
                source.IsTransitioning = (bool)e.OldValue;
                throw new InvalidOperationException("IsTransitioningReadOnly");
            }
        }
        #endregion public bool IsTransitioning

        #region public Brush DynamicBackground
        /// <summary>
        /// 
        /// </summary>
        public Brush DynamicBackground
        {
            get { return GetValue(DynamicBackgroundProperty) as Brush; }
            set { SetValue(DynamicBackgroundProperty, value); }
        }

        /// <summary>
        /// Identifies the DynamicBackground dependency property.
        /// </summary>
        public static readonly DependencyProperty DynamicBackgroundProperty =
            DependencyProperty.Register(
                "DynamicBackground",
                typeof(Brush),
                typeof(TransitioningBackgroundControl),
                new PropertyMetadata(null, OnDynamicBackgroundPropertyChanged));

        /// <summary>
        /// DynamicBackgroundProperty property changed handler.
        /// </summary>
        /// <param name="d">TransitioningBackgroundControl that changed its DynamicBackground.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnDynamicBackgroundPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TransitioningBackgroundControl source = d as TransitioningBackgroundControl;
            Brush value = e.NewValue as Brush;
            Brush old = e.OldValue as Brush;

            source.StartTransition(old, value);
        }
        #endregion public Brush DynamicBackground

        /// <summary>
        /// The storyboard that is used to transition old and new content.
        /// </summary>
        private Storyboard _currentTransition;

        /// <summary>
        /// Gets or sets the storyboard that is used to transition old and new content.
        /// </summary>
        private Storyboard CurrentTransition
        {
            get { return _currentTransition; }
            set
            {
                // decouple event
                if (_currentTransition != null)
                {
                    _currentTransition.Completed -= OnTransitionCompleted;
                }

                _currentTransition = value;

                if (_currentTransition != null)
                {
                    _currentTransition.Completed += OnTransitionCompleted;
                }
            }
        }

        #region public string Transition
        /// <summary>
        /// Gets or sets the name of the transition to use. These correspond
        /// directly to the VisualStates inside the PresentationStates group.
        /// </summary>
        public string Transition
        {
            get { return GetValue(TransitionProperty) as string; }
            set { SetValue(TransitionProperty, value); }
        }

        /// <summary>
        /// Identifies the Transition dependency property.
        /// </summary>
        public static readonly DependencyProperty TransitionProperty =
            DependencyProperty.Register(
                "Transition",
                typeof(string),
                typeof(TransitioningBackgroundControl),
                new PropertyMetadata(DefaultTransitionState, OnTransitionPropertyChanged));

        /// <summary>
        /// TransitionProperty property changed handler.
        /// </summary>
        /// <param name="d">TransitioningBackgroundControl that changed its Transition.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnTransitionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TransitioningBackgroundControl source = (TransitioningBackgroundControl)d;
            string oldTransition = e.OldValue as string;
            string newTransition = e.NewValue as string;

            if (source.IsTransitioning)
            {
                source.AbortTransition();
            }
  
            // find new transition
            Storyboard newStoryboard = source.GetStoryboard(newTransition);

            // unable to find the transition.
            if (newStoryboard == null)
            {
                // could be during initialization of xaml that presentationgroups was not yet defined
                if (VisualStates.TryGetVisualStateGroup(source, PresentationGroup) == null)
                {
                    // will delay check
                    source.CurrentTransition = null;
                }
                else
                {
                    // revert to old value
                    source.SetValue(TransitionProperty, oldTransition);

                    throw new ArgumentException(
                        string.Format(CultureInfo.CurrentCulture, "TransitionNotFound {0}", newTransition));
                }
            }
            else
            {
                source.CurrentTransition = newStoryboard;
            }
        }
        #endregion public string Transition

        #region public bool RestartTransitionOnContentChange
        /// <summary>
        /// Gets or sets a value indicating whether the current transition
        /// will be aborted when setting new content during a transition.
        /// </summary>
        public bool RestartTransitionOnContentChange
        {
            get { return (bool)GetValue(RestartTransitionOnContentChangeProperty); }
            set { SetValue(RestartTransitionOnContentChangeProperty, value); }
        }

        /// <summary>
        /// Identifies the RestartTransitionOnContentChange dependency property.
        /// </summary>
        public static readonly DependencyProperty RestartTransitionOnContentChangeProperty =
            DependencyProperty.Register(
                "RestartTransitionOnContentChange",
                typeof(bool),
                typeof(TransitioningBackgroundControl),
                new PropertyMetadata(false, OnRestartTransitionOnContentChangePropertyChanged));

        /// <summary>
        /// RestartTransitionOnContentChangeProperty property changed handler.
        /// </summary>
        /// <param name="d">TransitioningBackgroundControl that changed its RestartTransitionOnContentChange.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnRestartTransitionOnContentChangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TransitioningBackgroundControl) d).OnRestartTransitionOnContentChangeChanged((bool) e.OldValue, (bool) e.NewValue);
        }

        /// <summary>
        /// Called when the RestartTransitionOnContentChangeProperty changes.
        /// </summary>
        /// <param name="oldValue">The old value of RestartTransitionOnContentChange.</param>
        /// <param name="newValue">The new value of RestartTransitionOnContentChange.</param>
        protected virtual void OnRestartTransitionOnContentChangeChanged(bool oldValue, bool newValue)
        {
        }
        #endregion public bool RestartTransitionOnContentChange

        #region Events
        /// <summary>
        /// Occurs when the current transition has completed.
        /// </summary>
        public event RoutedEventHandler TransitionCompleted;
        #endregion Events

        /// <summary>
        /// Initializes a new instance of the <see cref="TransitioningBackgroundControl"/> class.
        /// </summary>
        public TransitioningBackgroundControl()
        {
            DefaultStyleKey = typeof(TransitioningBackgroundControl);
        }

        /// <summary>
        /// Builds the visual tree for the TransitioningBackgroundControl control 
        /// when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (IsTransitioning)
            {
                AbortTransition();
            }

            base.OnApplyTemplate();

            UIElement uie = this;
            while (uie != null)
            {
                uie = VisualTreeHelper.GetParent(uie) as UIElement;
                if (uie is UpdatingPanningLayer)
                {
                    _updatingPanningLayer = (UpdatingPanningLayer) uie;
                }
            }

            PreviousContentPresentationSite = GetTemplateChild(PreviousContentPresentationSitePartName) as Grid;
            CurrentContentPresentationSite = GetTemplateChild(CurrentContentPresentationSitePartName) as Grid;

            if (CurrentContentPresentationSite != null)
            {
                CurrentContentPresentationSite.Background = DynamicBackground;
            }

            // hookup currenttransition
            Storyboard transition = GetStoryboard(Transition);
            CurrentTransition = transition;
            if (transition == null)
            {
                string invalidTransition = Transition;
                // revert to default
                Transition = DefaultTransitionState;

                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, "TransitionNotFound {0}", invalidTransition));
            }

            VisualStateManager.GoToState(this, NormalState, false);
        }

        /// <summary>
        /// Starts the transition.
        /// </summary>
        /// <param name="old">The old content.</param>
        /// <param name="newBrush">The new content.</param>
        private void StartTransition(Brush old, Brush newBrush)
        {
            // both presenters must be available, otherwise a transition is useless.
            if (CurrentContentPresentationSite != null && PreviousContentPresentationSite != null)
            {
                CurrentContentPresentationSite.Background = newBrush;
                PreviousContentPresentationSite.Background = old;

                // and start a new transition
                if (!IsTransitioning || RestartTransitionOnContentChange)
                {
                    ImageBrush newImageBrush = newBrush as ImageBrush;
                    if (newImageBrush != null)
                    {
                        BitmapImage bmp = newImageBrush.ImageSource as BitmapImage;
                        if (bmp != null)
                        {
                            VisualStateManager.GoToState(this, "LoadingState", false);
                            bmp.ImageOpened += (i, ie) => TransitionNow();
                            return;
                        }
                    }

                    TransitionNow();
                }
            }
        }

            private void TransitionNow()
            {
                    IsTransitioning = true;
                    VisualStateManager.GoToState(this, NormalState, false);
                    VisualStateManager.GoToState(this, Transition, true);

            }

        /// <summary>
        /// Handles the Completed event of the transition storyboard.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnTransitionCompleted(object sender, EventArgs e)
        {
            if (_updatingPanningLayer != null)
            {
                _updatingPanningLayer.RefreshEdges();
            }

            AbortTransition();

            RoutedEventHandler handler = TransitionCompleted;
            if (handler != null)
            {
                handler(this, new RoutedEventArgs());
            }
        }

        /// <summary>
        /// Aborts the transition and releases the previous content.
        /// </summary>
        public void AbortTransition()
        {
            // go to normal state and release our hold on the old content.
            VisualStateManager.GoToState(this, NormalState, false);
            IsTransitioning = false;
            if (PreviousContentPresentationSite != null)
            {
                PreviousContentPresentationSite.Background = null;
                // PreviousContentPresentationSite.Content = null;
            }
        }

        /// <summary>
        /// Attempts to find a storyboard that matches the newTransition name.
        /// </summary>
        /// <param name="newTransition">The new transition.</param>
        /// <returns>A storyboard or null, if no storyboard was found.</returns>
        private Storyboard GetStoryboard(string newTransition)
        {
            VisualStateGroup presentationGroup = VisualStates.TryGetVisualStateGroup(this, PresentationGroup);
            Storyboard newStoryboard = null;
            if (presentationGroup != null)
            {
                newStoryboard = presentationGroup.States
                    .OfType<VisualState>()
                    .Where(state => state.Name == newTransition)
                    .Select(state => state.Storyboard)
                    .FirstOrDefault();
            }
            return newStoryboard;
        }

        /// <summary>
        /// Names and helpers for visual states in the controls.
        /// </summary>
        internal static class VisualStates
        {
            /// <summary>
            /// Use VisualStateManager to change the visual state of the control.
            /// </summary>
            /// <param name="control">
            /// Control whose visual state is being changed.
            /// </param>
            /// <param name="useTransitions">
            /// A value indicating whether to use transitions when updating the
            /// visual state, or to snap directly to the new visual state.
            /// </param>
            /// <param name="stateNames">
            /// Ordered list of state names and fallback states to transition into.
            /// Only the first state to be found will be used.
            /// </param>
            internal static void GoToState(Control control, bool useTransitions, params string[] stateNames)
            {
                Debug.Assert(control != null, "control should not be null!");
                Debug.Assert(stateNames != null, "stateNames should not be null!");
                Debug.Assert(stateNames.Length > 0, "stateNames should not be empty!");

                foreach (string name in stateNames)
                {
                    if (VisualStateManager.GoToState(control, name, useTransitions))
                    {
                        break;
                    }
                }
            }

            /// <summary>
            /// Gets the implementation root of the Control.
            /// </summary>
            /// <param name="dependencyObject">The DependencyObject.</param>
            /// <remarks>
            /// Implements Silverlight's corresponding internal property on Control.
            /// </remarks>
            /// <returns>Returns the implementation root or null.</returns>
            internal static FrameworkElement GetImplementationRoot(DependencyObject dependencyObject)
            {
                Debug.Assert(dependencyObject != null, "DependencyObject should not be null.");
                return (1 == VisualTreeHelper.GetChildrenCount(dependencyObject)) ?
                    VisualTreeHelper.GetChild(dependencyObject, 0) as FrameworkElement :
                    null;
            }

            /// <summary>
            /// This method tries to get the named VisualStateGroup for the 
            /// dependency object. The provided object's ImplementationRoot will be 
            /// looked up in this call.
            /// </summary>
            /// <param name="dependencyObject">The dependency object.</param>
            /// <param name="groupName">The visual state group's name.</param>
            /// <returns>Returns null or the VisualStateGroup object.</returns>
            internal static VisualStateGroup TryGetVisualStateGroup(DependencyObject dependencyObject, string groupName)
            {
                FrameworkElement root = GetImplementationRoot(dependencyObject);
                if (root == null)
                {
                    return null;
                }

                return VisualStateManager.GetVisualStateGroups(root)
                    .OfType<VisualStateGroup>()
                    .Where(group => string.CompareOrdinal(groupName, group.Name) == 0)
                    .FirstOrDefault();
            }
        }
    }
}