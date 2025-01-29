// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// The ARC geometric drawing operation provides the capability of
/// dra wing circles, segments of circles, and curvilinear splines. For circles and
/// segments of circles, an arc is drawn from a start point to an end point through
/// an intermediate point on the arc.Drawing of a circle results when the start
/// and end points are coincident; the intermediate point defines the diameter of
/// the circle, and therefore is the midpoint on the arc between the start and end
/// points.A segment of a circle is drawn when the start and end points are not
/// coincident.
/// 
/// The start point is specified either explicitly within the ARC command or as
/// the current drawing point. The intermediate point is described as a relative
/// displacement from the start point.The end point is specified as a relative
/// displacement from the intermediate point. It is good practice, in order to
/// minimize error, to always specify the intermediate point on the arc as being
/// approximately midway between the start and end points.
/// 
/// If the three drawing points are colinear, a line is drawn from the start point to
/// the end point, except for the error condition in which the intermediate point
/// does not lie between the start and end points.If the end point is omitted, it is
/// taken to be coincident witn the start point and a circle is drawn.Note that
/// the arc may not be specified so that any portion of it lies outside the unit
/// screen (see 5.3.1.1). At the completion of drawing an arc, the drawing point is
/// coincident with the end point.
/// 
/// An arc may be either filled or outlined.Outlined arcs are drawn in the current
/// color(s), have a width that is determined by the logical pel size, and a line
/// texture that is as specified by the TEXTURE command.The chord that joins
/// the start and end points is not considered part of the outline and, as such, is
/// not drawn.
/// 
/// For filled arcs, the area enclosed by the outline and the chord (including the
/// region of the outline and the chord traced by the logical pel) is filled in the
/// current color(s) with the texture pattern specified in the TEXTURE command.
/// 
/// The stroke width of the chord is affected by the logical pe4 but the chord is
/// not considered a part of the arc and, as such, is not highlighted if highlight
/// mode is selected (see 5.3.2.4.3 and Figure 40.)
/// 
/// Drawing of a curvilinear spline results when more than three points are
/// specified. The last point specified is the end point.The drawing point at the
/// completion of drawing a spline is the end point.The minimum implementation
/// of the spline shall be a series of lines connecting the start, intermediate, and
/// end points of the spline. The display device may draw a smoother spline, but
/// the shape of this spline and the characteristics of the algorithm used are
/// implementation-dependent. The complete algorithm for a curvilinear spline is
/// reserved for future standardization. All the attributes described above for
/// circles and segments of circles(colinear points, points outside the unit screen,
/// fill, and outline) apply to splines.In the case of a filled spline, the spline and
/// the chord(the line that joins the start and end points) must enclose a single
/// area, ie, no portion of the spline outline or chord may cross any other portion
/// of the spline or chord. The maximum number of points permitted to describe a
/// spline is implementation-dependent, and shall be at least 256 points.
/// </summary>
public abstract class ArcCommand : FillableGeometricDrawingCommandBase
{
    public Vector3 StartPoint { get; internal set; }

    public Vector3 IntermediatePointDisplacement { get; internal set; }

    public Vector3 EndPointDisplacement { get; internal set; }

    public ArcCommand(bool isSet, NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        Vertices = ProcessVertices(operands);

        if (!isSet)
        {
            if (operands.Count == State.MultiByteValue * 2)
            {
                SetPen(State.Pen);

                StartPoint = State.Pen;
                IntermediatePointDisplacement = StartPoint + Vertices[0];
                EndPointDisplacement = IntermediatePointDisplacement + Vertices[1];
            }
            else if (operands.Count == State.MultiByteValue)
            {
                // Circle
                SetPen(State.Pen);

                StartPoint = State.Pen;
                IntermediatePointDisplacement = StartPoint + Vertices[0];
                EndPointDisplacement = State.Pen;
            }
            else
            {
                IsValid = false;

                return;
            }
        }
        else
        {
            if (operands.Count == State.MultiByteValue * 3)
            {
                SetPen(Vertices[0]);

                StartPoint = Vertices[0];
                IntermediatePointDisplacement = StartPoint + Vertices[1];
                EndPointDisplacement = IntermediatePointDisplacement + Vertices[2];

                SetPen(Vertices[2]);
            }
            else if (operands.Count == State.MultiByteValue * 2)
            {
                SetPen(Vertices[0]);

                // Circle
                StartPoint = Vertices[0];
                IntermediatePointDisplacement = StartPoint + Vertices[1];
                EndPointDisplacement = Vertices[0];
            }
            else
            {
                IsValid = false;

                return;
            }
        }

        SetPen(EndPointDisplacement);
    }

}
