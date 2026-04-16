// spirograph.td — Procedural spirograph pattern
#coord fractions

proc spoke(cx, cy, len, angle_step, count) {
  move cx cy
  repeat count {
    forward len
    turn angle_step
  }
}

with color 3 {
  spoke 0.5 0.4 0.08 37 80
}

with color 1 {
  spoke 0.5 0.4 0.12 73 50
}

with color 5 {
  spoke 0.5 0.4 0.05 137 100
}
