// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

namespace UnityEngine.UIElements
{
    /// <summary>
    /// Interface for focus events.
    /// </summary>
    /// <remarks>
    /// Refer to the [[wiki:UIE-Focus-Events|Focus events]] manual page for more information and examples.
    /// </remarks>
    public interface IFocusEvent
    {
        /// <summary>
        /// Related target. See implementation for specific meaning.
        /// </summary>
        Focusable relatedTarget { get; }

        /// <summary>
        /// Direction of the focus change.
        /// This corresponds to the FocusChangeDirection used by the focus ring.
        /// </summary>
        /// <remarks>
        /// The <see cref="IFocusRing"/> implementation determines what focus events are generated
        /// as a consequence of other input events.
        /// Focus events generally occur after any of the following situations:
        ///
        ///- A <see cref="NavigationMoveEvent"/>
        ///- A <see cref="PointerDownEvent"/>
        ///- Calling an element's <see cref="Focusable.Focus()"/> or <see cref="Focusable.Blur()"/> methods
        ///
        /// The direction of the focus change contains information about the cause of the Focus event.
        /// It can be null if the focus change didn't happen as a consequence of another event.
        /// </remarks>
        /// <seealso cref="VisualElementFocusChangeDirection" />
        /// <seealso cref="IFocusRing" />
        FocusChangeDirection direction { get; }
    }

    /// <summary>
    /// Base class for focus related events.
    /// </summary>
    /// <remarks>
    /// Refer to the [[wiki:UIE-Focus-Events|Focus events]] manual page for more information and examples.
    /// </remarks>
    [EventCategory(EventCategory.Focus)]
    public abstract class FocusEventBase<T> : EventBase<T>, IFocusEvent where T : FocusEventBase<T>, new()
    {
        /// <summary>
        /// For FocusOut and Blur events, contains the element that gains the focus. For FocusIn and Focus events, contains the element that loses the focus.
        /// </summary>
        public Focusable relatedTarget { get; private set; }

        /// <summary>
        /// See <see cref="IFocusEvent.direction"/>.
        /// </summary>
        public FocusChangeDirection direction { get; private set; }

        /// <summary>
        /// The focus controller that emitted the event.
        /// </summary>
        protected FocusController focusController { get; private set; }
        internal bool IsFocusDelegated { get; private set; }

        /// <summary>
        /// Resets the event members to their initial values.
        /// </summary>
        protected override void Init()
        {
            base.Init();
            LocalInit();
        }

        void LocalInit()
        {
            propagation = EventPropagation.TricklesDown;
            relatedTarget = null;
            direction = FocusChangeDirection.unspecified;
            focusController = null;
        }

        /// <summary>
        /// Gets an event from the event pool and initializes it with the given values. Use this function instead of creating new events. Events obtained using this method need to be released back to the pool. You can use `Dispose()` to release them.
        /// </summary>
        /// <param name="target">The event target.</param>
        /// <param name="relatedTarget">The related target.</param>
        /// <param name="direction">The direction of the focus change.</param>
        /// <param name="focusController">The object that manages the focus.</param>
        /// <returns>An initialized event.</returns>
        public static T GetPooled(IEventHandler target, Focusable relatedTarget, FocusChangeDirection direction, FocusController focusController, bool bIsFocusDelegated = false)
        {
            T e = GetPooled();
            e.elementTarget = (VisualElement) target;
            e.relatedTarget = relatedTarget;
            e.direction = direction;
            e.focusController = focusController;
            e.IsFocusDelegated = bIsFocusDelegated;
            return e;
        }

        protected FocusEventBase()
        {
            LocalInit();
        }
    }

    /// <summary>
    /// Event sent immediately before an element loses focus. This event trickles down and bubbles up.
    /// </summary>
    public class FocusOutEvent : FocusEventBase<FocusOutEvent>
    {
        static FocusOutEvent()
        {
            SetCreateFunction(() => new FocusOutEvent());
        }

        /// <summary>
        /// Resets the event members to their initial values.
        /// </summary>
        protected override void Init()
        {
            base.Init();
            LocalInit();
        }

        void LocalInit()
        {
            propagation = EventPropagation.Bubbles | EventPropagation.TricklesDown;
        }

        /// <summary>
        /// Constructor. Avoid creating new event instances. Instead, use GetPooled() to get an instance from a pool of reusable event instances.
        /// </summary>
        public FocusOutEvent()
        {
            LocalInit();
        }

        protected internal override void PostDispatch(IPanel panel)
        {
            if (relatedTarget == null)
            {
                focusController.ProcessPendingFocusChange(null);
            }

            base.PostDispatch(panel);
        }
    }

    /// <summary>
    /// Event sent immediately after an element has lost focus. This event trickles down and does not bubbles up.
    /// </summary>
    public class BlurEvent : FocusEventBase<BlurEvent>
    {
        static BlurEvent()
        {
            SetCreateFunction(() => new BlurEvent());
        }
    }

    /// <summary>
    /// Event sent immediately before an element gains focus. This event trickles down and bubbles up.
    /// </summary>
    /// <example>
    /// <code source="../../Tests/UIElementsExamples/Assets/Examples/FocusExample.cs"/>
    /// </example>
    public class FocusInEvent : FocusEventBase<FocusInEvent>
    {
        static FocusInEvent()
        {
            SetCreateFunction(() => new FocusInEvent());
        }

        /// <summary>
        /// Resets the event members to their initial values.
        /// </summary>
        protected override void Init()
        {
            base.Init();
            LocalInit();
        }

        void LocalInit()
        {
            propagation = EventPropagation.Bubbles | EventPropagation.TricklesDown;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public FocusInEvent()
        {
            LocalInit();
        }

        protected internal override void PostDispatch(IPanel panel)
        {
            focusController.ProcessPendingFocusChange(elementTarget);

            base.PostDispatch(panel);
        }
    }

    /// <summary>
    /// Event sent immediately after an element has gained focus. This event trickles down and does not bubbles up.
    /// </summary>
    public class FocusEvent : FocusEventBase<FocusEvent>
    {
        static FocusEvent()
        {
            SetCreateFunction(() => new FocusEvent());
        }
    }
}
