using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using XamlX.Transform;
using XamlX.TypeSystem;
using Visitor = XamlX.Ast.XamlXAstVisitorDelegate;

namespace XamlX.Ast
{
    public class XamlAstClrTypeReference : XamlAstNode, IXamlAstTypeReference
    {
        public IXamlType Type { get; }

        public XamlAstClrTypeReference(IXamlLineInfo lineInfo, IXamlType type) : base(lineInfo)
        {
            Type = type;
        }

        public override string ToString() => Type.GetFqn();
    }

    public class XamlAstClrPropertyReference : XamlAstNode, IXamlAstPropertyReference
    {
        public IXamlProperty Property { get; set; }

        public XamlAstClrPropertyReference(IXamlLineInfo lineInfo, IXamlProperty property) : base(lineInfo)
        {
            Property = property;
        }

        public override string ToString() => Property.PropertyType.GetFqn() + "." + Property.Name;
    }

    public class XamlPropertyAssignmentNode : XamlAstNode, IXamlAstManipulationNode
    {
        public IXamlProperty Property { get; set; }
        public IXamlAstValueNode Value { get; set; }

        public XamlPropertyAssignmentNode(IXamlLineInfo lineInfo,
            IXamlProperty property, IXamlAstValueNode value)
            : base(lineInfo)
        {
            Property = property;
            Value = value;
        }
    }
    
    public class XamlPropertyValueManipulationNode : XamlAstNode, IXamlAstManipulationNode
    {
        public IXamlProperty Property { get; set; }
        public IXamlAstManipulationNode Manipulation { get; set; }
        public XamlPropertyValueManipulationNode(IXamlLineInfo lineInfo, 
            IXamlProperty property, IXamlAstManipulationNode manipulation) 
            : base(lineInfo)
        {
            Property = property;
            Manipulation = manipulation;
        }

        public override void VisitChildren(XamlXAstVisitorDelegate visitor)
        {
            Manipulation = (IXamlAstManipulationNode) Manipulation.Visit(visitor);
        }
    }

    public abstract class XamlXInstanceMethodCallBaseNode : XamlAstNode
    {
        public IXamlMethod Method { get; set; }
        public List<IXamlAstValueNode> Arguments { get; set; }
        public XamlXInstanceMethodCallBaseNode(IXamlLineInfo lineInfo, 
            IXamlMethod method, IEnumerable<IXamlAstValueNode> args) 
            : base(lineInfo)
        {
            Method = method;
            Arguments = args?.ToList() ?? new List<IXamlAstValueNode>();
        }

        public override void VisitChildren(XamlXAstVisitorDelegate visitor)
        {
            VisitList(Arguments, visitor);
        }
    }
    
    public class XamlXInstanceNoReturnMethodCallNode : XamlXInstanceMethodCallBaseNode, IXamlAstManipulationNode
    {
        public XamlXInstanceNoReturnMethodCallNode(IXamlLineInfo lineInfo, IXamlMethod method, IEnumerable<IXamlAstValueNode> args)
            : base(lineInfo, method, args)
        {
        }
    }
    
    public class XamlXStaticReturnMethodCallNode : XamlXInstanceMethodCallBaseNode, IXamlAstValueNode
    {
        public XamlXStaticReturnMethodCallNode(IXamlLineInfo lineInfo, IXamlMethod method, IEnumerable<IXamlAstValueNode> args)
            : base(lineInfo, method, args)
        {
            Type = new XamlAstClrTypeReference(lineInfo, method.ReturnType);
        }

        public IXamlAstTypeReference Type { get; }
    }

    public class XamlManipulationGroupNode : XamlAstNode, IXamlAstManipulationNode
    {
        public List<IXamlAstManipulationNode> Children { get; set; } = new List<IXamlAstManipulationNode>();
        public XamlManipulationGroupNode(IXamlLineInfo lineInfo) : base(lineInfo)
        {
        }

        public override void VisitChildren(XamlXAstVisitorDelegate visitor) => VisitList(Children, visitor);
    }
}