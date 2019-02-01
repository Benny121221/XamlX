using System.Collections.Generic;
using System.Linq;
using XamlX.Ast;
using XamlX.TypeSystem;

namespace XamlX.Transform.Transformers
{
    public class XamlXContentTransformer : IXamlXAstTransformer
    {
        public IXamlXAstNode Transform(XamlXAstTransformationContext context, IXamlXAstNode node)
        {
            if (node is XamlXAstNewInstanceNode ni)
            {
                var valueIndexes = new List<int>();
                for (var c = 0; c < ni.Children.Count; c++)
                    if (ni.Children[c] is IXamlXAstValueNode)
                        valueIndexes.Add(c);

                if (valueIndexes.Count == 0)
                    return node;
                var type = ni.Type.GetClrType();
                IXamlXAstValueNode VNode(int ind) => (IXamlXAstValueNode) ni.Children[ind];

                var contentProperty = context.Configuration.FindContentProperty(type);
                if (contentProperty == null)
                {
                    foreach (var ind in valueIndexes)
                        if (context.Configuration.TryCallAdd(type, VNode(ind), out var addCall))
                            ni.Children[ind] = addCall;
                        else
                            throw new XamlXLoadException(
                                $"Type `{type.GetFqn()} does not have content property and suitable Add({VNode(ind).Type.GetClrType().GetFqn()}) not found",
                                VNode(ind));
                }
                else
                    XamlXTransformHelpers.GeneratePropertyAssignments(context, contentProperty, valueIndexes.Count,
                        num => VNode(valueIndexes[num]),
                        (i, v) => ni.Children[valueIndexes[i]] = v);
            }

            return node;
        }
    }
}