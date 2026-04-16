// star.td — Five-pointed star using turtle graphics
#coord fractions

proc star(x, y, size) {
  move x y
  repeat 5 {
    forward size
    turn 144
  }
}

with color 4 {
  star 0.35 0.55 0.3
}

with color 2 {
  star 0.5 0.25 0.15
}
