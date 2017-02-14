model damped_oscillator
  Modelica.Mechanics.Translational.Components.Fixed fixed annotation(
    Placement(visible = true, transformation(origin = {0, 56}, extent = {{-10, -10}, {10, 10}}, rotation = 180)));
  Modelica.Mechanics.Translational.Components.SpringDamper springDamper(c = 10, d = 0.5) annotation(
    Placement(visible = true, transformation(origin = {0, 24}, extent = {{-10, -10}, {10, 10}}, rotation = -90)));
  Modelica.Mechanics.Translational.Components.Mass mass(m = 1, s(fixed = true, start = -0.1), v(fixed = true, start = 0)) annotation(
    Placement(visible = true, transformation(origin = {0, -12}, extent = {{-10, -10}, {10, 10}}, rotation = -90)));
equation
  connect(springDamper.flange_b, mass.flange_a) annotation(
    Line(points = {{0, 14}, {0, 14}, {0, -2}, {0, -2}}, color = {0, 127, 0}));
  connect(fixed.flange, springDamper.flange_a) annotation(
    Line(points = {{0, 56}, {0, 56}, {0, 34}, {0, 34}}, color = {0, 127, 0}));
  annotation(
    uses(Modelica(version = "3.2.2")),
    experiment(StartTime = 0, StopTime = 10, Tolerance = 1e-06, Interval = 0.02));
end damped_oscillator;