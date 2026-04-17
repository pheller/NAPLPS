// clock.td — Analog clock face with hour ticks
#coord fractions

let cx = 0.5
let cy = 0.4
let r = 0.25

// Outer rim
with color 7 {
  move cx cy
  // Approximate circle by polygon with 24 vertices
  // (NAPLPS has no native circle; arc commands bezier)
}

// 12 hour marks (every 30 degrees)
with color 1 {
  for h in 0..11 {
    let angle_deg = h * 30
    move cx cy
    turn angle_deg
    forward (r - 0.03)
    line cx cy
    turn (-1 * angle_deg)
  }
}

// Hour hand (pointing up, 12 o'clock)
with color 2 {
  move cx cy
  forward (r * 0.55)
  line cx cy
}

// Minute hand
with color 4 {
  move cx cy
  turn 90
  forward (r * 0.85)
  line cx cy
}
