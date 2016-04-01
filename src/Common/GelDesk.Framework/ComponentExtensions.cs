using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GelDesk
{
    public static class ComponentExtensions
    {
        public static TComponent FindUp<TComponent>(this ComponentObject component)
            where TComponent : class
        {
            return (TComponent)FindUp(component, typeof(TComponent));
        }
        public static object FindUp(this ComponentObject component, Type componentType)
        {
            if (component == null || component.Parent == null)
                return null;
            var found = component.Parent.Components
                .FirstOrDefault(child => child.GetType() == componentType);
            if (found != null)
                return found;
            return FindUp(component.Parent, componentType);
        }
        public static bool TryGetView<TView>(this ComponentObject component, out TView view)
            where TView : class
        {
            var vc = component as IViewController;
            if (vc == null)
            {
                view = null;
                return false;
            }
            view = vc.GetViews().OfType<TView>().FirstOrDefault();
            return view != null;
        }
        public static IEnumerable<TView> ViewsOfType<TView>(this ComponentsCollection components)
            where TView: class
        {
            return components.OfType<IViewController>()
                .SelectMany(vc => vc.GetViews())
                .OfType<TView>();
        }
    }
}
