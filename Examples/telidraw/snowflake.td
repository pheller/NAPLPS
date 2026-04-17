// snowflake.td — 6-fold symmetric Koch-style snowflake
#coord fractions

proc branch(len) {
  forward len
  turn 60
  forward (len / 2)
  turn -120
  forward (len / 2)
  turn 60
  back (len / 2 + len / 2)
  back len
}

with color 4 {
  for i in 0..5 {
    move 0.5 0.4
    turn (i * 60)
    branch 0.15
    turn (-1 * i * 60)
  }
}

with color 7 {
  move 0.45 0.7
  text "SNOWFLAKE"
}
