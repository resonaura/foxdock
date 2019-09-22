using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace FoxDock
{
    public class BrushAnimation : AnimationTimeline
    {
        public override Type TargetPropertyType
        {
            get
            {
                return typeof(System.Windows.Media.Brush);
            }
        }

        public override object GetCurrentValue(object defaultOriginValue,
                                               object defaultDestinationValue,
                                               AnimationClock animationClock)
        {
            return GetCurrentValue(defaultOriginValue as System.Windows.Media.Brush,
                                   defaultDestinationValue as System.Windows.Media.Brush,
                                   animationClock);
        }
        public object GetCurrentValue(System.Windows.Media.Brush defaultOriginValue,
                                      System.Windows.Media.Brush defaultDestinationValue,
                                      AnimationClock animationClock)
        {
            if (!animationClock.CurrentProgress.HasValue)
            {
                return System.Windows.Media.Brushes.Transparent;
            }

            //use the standard values if From and To are not set 
            //(it is the value of the given property)
            defaultOriginValue = this.From ?? defaultOriginValue;
            defaultDestinationValue = this.To ?? defaultDestinationValue;

            if (animationClock.CurrentProgress.Value == 0)
            {
                return defaultOriginValue;
            }

            if (animationClock.CurrentProgress.Value == 1)
            {
                return defaultDestinationValue;
            }

            return new VisualBrush(new Border()
            {
                Width = 1,
                Height = 1,
                Background = defaultOriginValue,
                Child = new Border()
                {
                    Background = defaultDestinationValue,
                    Opacity = animationClock.CurrentProgress.Value,
                }
            });
        }

        protected override Freezable CreateInstanceCore()
        {
            return new BrushAnimation();
        }

        //we must define From and To, AnimationTimeline does not have this properties
        public System.Windows.Media.Brush From
        {
            get { return (System.Windows.Media.Brush)GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }
        public System.Windows.Media.Brush To
        {
            get { return (System.Windows.Media.Brush)GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }

        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register("From", typeof(System.Windows.Media.Brush), typeof(BrushAnimation));
        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register("To", typeof(System.Windows.Media.Brush), typeof(BrushAnimation));
    }
}
