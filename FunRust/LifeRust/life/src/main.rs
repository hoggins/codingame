use std::io;
use std::ops::Index;

macro_rules! parse_input {
  ($x:expr, $t:ident) => {
    $x.trim().parse::<$t>().unwrap()
  };
}

/**
 * Bring data on patient samples from the diagnosis machine to the laboratory with enough molecules to produce medicine!
 **/
fn main() {
  let mut input_line = String::new();
  io::stdin().read_line(&mut input_line).unwrap();
  let project_count = parse_input!(input_line, i32);
  for i in 0..project_count as usize {
    let mut input_line = String::new();
    io::stdin().read_line(&mut input_line).unwrap();
    let inputs = input_line.split(" ").collect::<Vec<_>>();
    let a = parse_input!(inputs[0], i32);
    let b = parse_input!(inputs[1], i32);
    let c = parse_input!(inputs[2], i32);
    let d = parse_input!(inputs[3], i32);
    let e = parse_input!(inputs[4], i32);
  }

  // game loop
  loop {
    let mut robots: Vec<Robot> = Vec::new();
    for i in 0..2 as usize {
      let mut input_line = String::new();
      io::stdin().read_line(&mut input_line).unwrap();
      let inputs = input_line.split(" ").collect::<Vec<_>>();
      let robot = Robot {
        target: inputs[0].trim().to_string(),
        eta: parse_input!(inputs[1], i32),
        score: parse_input!(inputs[2], i32),
        storage_a: parse_input!(inputs[3], i32),
        storage_b: parse_input!(inputs[4], i32),
        storage_c: parse_input!(inputs[5], i32),
        storage_d: parse_input!(inputs[6], i32),
        storage_e: parse_input!(inputs[7], i32),
        expertise_a: parse_input!(inputs[8], i32),
        expertise_b: parse_input!(inputs[9], i32),
        expertise_c: parse_input!(inputs[10], i32),
        expertise_d: parse_input!(inputs[11], i32),
        expertise_e: parse_input!(inputs[12], i32),
      };
      robots.push(robot);
    }

    let mut input_line = String::new();
    io::stdin().read_line(&mut input_line).unwrap();
    let inputs = input_line.split(" ").collect::<Vec<_>>();
    let available_a = parse_input!(inputs[0], i32);
    let available_b = parse_input!(inputs[1], i32);
    let available_c = parse_input!(inputs[2], i32);
    let available_d = parse_input!(inputs[3], i32);
    let available_e = parse_input!(inputs[4], i32);

    let mut input_line = String::new();
    io::stdin().read_line(&mut input_line).unwrap();
    let sample_count = parse_input!(input_line, i32);
    let mut samples: Vec<Sample> = Vec::new();
    for i in 0..sample_count as usize {
      let mut input_line = String::new();
      io::stdin().read_line(&mut input_line).unwrap();
      let inputs = input_line.split(" ").collect::<Vec<_>>();
      let sample = Sample {
        sample_id: parse_input!(inputs[0], i32),
        carried_by: parse_input!(inputs[1], i32),
        rank: parse_input!(inputs[2], i32),
        expertise_gain: inputs[3].trim().to_string(),
        health: parse_input!(inputs[4], i32),
        cost_a: parse_input!(inputs[5], i32),
        cost_b: parse_input!(inputs[6], i32),
        cost_c: parse_input!(inputs[7], i32),
        cost_d: parse_input!(inputs[8], i32),
        cost_e: parse_input!(inputs[9], i32),
      };

      samples.push(sample);
    }

    let myself = robots.index(0);
    let cur_sample = samples.iter().find(|&x| x.carried_by == 0);

    if cur_sample.is_none() {
      if myself.target != "DIAGNOSIS" {
        println!("GOTO DIAGNOSIS");
      } else {
        let sample = samples
          .iter()
          .filter(|&x| x.carried_by == -1)
          .min_by_key(|&x| x.cost_a + x.cost_b + x.cost_c + x.cost_d + x.cost_e)
          .unwrap()
          .sample_id;
        println!("CONNECT {}", sample)
      }
      return;
    }

    let cur_sample = cur_sample.unwrap();
    let storage_cap = myself.storage_a + myself.storage_b + myself.storage_c + myself.storage_e;
    let sample_cap = cur_sample.cost_a + cur_sample.cost_b + cur_sample.cost_c + cur_sample.cost_e;
    if storage_cap != sample_cap {
      if myself.target != "MOLECULES" {}
    }

    // Write an action using println!("message...");
    // To debug: eprintln!("Debug message...");

    println!("GOTO DIAGNOSIS");
  }
}

struct Robot {
  target: String,
  eta: i32,
  score: i32,
  storage_a: i32,
  storage_b: i32,
  storage_c: i32,
  storage_d: i32,
  storage_e: i32,
  expertise_a: i32,
  expertise_b: i32,
  expertise_c: i32,
  expertise_d: i32,
  expertise_e: i32,
}

struct Sample {
  sample_id: i32,
  carried_by: i32,
  rank: i32,
  expertise_gain: String,
  health: i32,
  cost_a: i32,
  cost_b: i32,
  cost_c: i32,
  cost_d: i32,
  cost_e: i32,
}

impl Sample {}
