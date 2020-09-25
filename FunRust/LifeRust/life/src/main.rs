use crate::entities::*;
use std::time::Instant;

macro_rules! parse_input {
  ($x:expr, $t:ident) => {
    $x.trim().parse::<$t>().unwrap()
  };
}

const SAMPLES: &str = "SAMPLES";
const DIAGNOSIS: &str = "DIAGNOSIS";
const MOLECULES: &str = "MOLECULES";
const LABORATORY: &str = "LABORATORY";
const START_POS: &str = "START_POS";
const MOLECULE_A: &str = "A";
const MOLECULE_B: &str = "B";
const MOLECULE_C: &str = "C";
const MOLECULE_D: &str = "D";
const MOLECULE_E: &str = "E";
const MOLECULE_0: &str = "0";

const NEG_INFINITY: f64 = std::f64::NEG_INFINITY;
const INFINITY: f64 = std::f64::INFINITY;

static mut FAKE_SAMPLE_ID: i32 = 5000;

static mut MINIMAX_COUNT: i64 = 0;

unsafe fn minimax_count_inc() -> i64 {
  let ret = MINIMAX_COUNT;
  MINIMAX_COUNT += 1;
  ret
}

fn main() {
  input::parse_projects();
  // game loop
  loop {
    let now = Instant::now();
    let mut minimax = unsafe { MINIMAX_COUNT };

    let robots = input::parse_robots();
    let available = input::parse_available();
    let samples = input::parse_samples();
    let mut gs = GameState {
      robots: robots,
      samples: samples,
      available: available,
    };

    eprintln!("storage {:?}", gs.robots[0].storage);
    eprintln!("expertise {:?}", gs.robots[0].expertise);
    eprintln!("available {:?}", gs.available);
    eprintln!("");
    let robot = &gs.robots[0];
    let mut robot_bank = robot.storage + robot.expertise;
    let mut bank = robot.expertise + robot.storage + gs.available;
    for s in heuristic::sample_get_sorted(&gs, 0) {
      //eprintln!("cost {:?}", s.cost);
      let to_take = robot_bank.dificite(&s.cost).abs();
      let missing = bank.dificite(&s.cost);
      bank = (bank - s.cost).clamp_negative();
      robot_bank = (robot_bank - s.cost).clamp_negative();
      eprintln!("to_take {:?}", to_take);
      eprintln!("missing {:?}", missing);
      //eprintln!(        "dif {:?}",        gs.available - gs.robots[0].missing_cost(&s.cost)      );
      //eprintln!("");
    }

    let action = minimax::decide_move(&mut gs);

    if let Some(action) = action {
      action.execute();
    } else {
      println!("WAIT");
    }

    // Write an action using println!("message...");
    // To debug: eprintln!("Debug message...");

    //println!("GOTO DIAGNOSIS");
    minimax = unsafe { MINIMAX_COUNT } - minimax;
    eprintln!("sim {}", minimax);
    eprintln!("in {}", now.elapsed().as_millis());
  }
}

fn get_move_cost(from: &str, to: &str) -> i32 {
  let pair = (from, to);
  match pair {
    (x, y) if x == y => 0,
    (SAMPLES, _) => 3,
    (DIAGNOSIS, LABORATORY) => 4,
    (DIAGNOSIS, _) => 3,
    (MOLECULES, _) => 3,
    (LABORATORY, DIAGNOSIS) => 4,
    (LABORATORY, _) => 3,
    _ => 2,
  }
}

fn sample_get_min_score(rank: i32) -> i32 {
  match rank {
    3 => 20,
    2 => 10,
    _ => 1,
  }
}

mod entities {
  use std::cmp;
  use std::ops::Add;
  use std::ops::Sub;

  pub struct GameState {
    pub robots: Vec<Robot>,
    pub samples: Vec<Sample>,
    pub available: MoleculeSet,
  }

  pub struct Robot {
    pub target: &'static str,
    pub eta: i32,
    pub score: i32,
    pub storage: MoleculeSet,
    pub expertise: MoleculeSet,
  }

  impl Robot {
    pub fn missing_cost(&self, s: &MoleculeSet) -> MoleculeSet {
      ((*s - self.storage) - self.expertise).clamp_negative()
    }
  }

  pub struct Sample {
    pub sample_id: i32,
    pub carried_by: i32,
    pub rank: i32,
    pub expertise_gain: &'static str,
    pub health: i32,
    pub cost: MoleculeSet,
    pub is_fake_diagnose: bool,
  }

  impl Sample {
    pub fn is_undiagnosed(&self) -> bool {
      self.cost.a == -1
      // && self.cost_b == 0 && self.cost_c == 0 && self.cost_d == 0 && self.cost_e == 0
    }
    pub fn is_diagnosed(&self) -> bool {
      self.cost.a >= 0
    }
  }

  #[derive(Debug, Clone, Copy)]
  pub struct MoleculeSet {
    pub a: i32,
    pub b: i32,
    pub c: i32,
    pub d: i32,
    pub e: i32,
  }

  impl MoleculeSet {
    pub fn new(val: i32) -> MoleculeSet {
      MoleculeSet {
        a: val,
        b: val,
        c: val,
        d: val,
        e: val,
      }
    }

    pub fn parse(inputs: &[&str]) -> MoleculeSet {
      MoleculeSet {
        a: parse_input!(inputs[0], i32),
        b: parse_input!(inputs[1], i32),
        c: parse_input!(inputs[2], i32),
        d: parse_input!(inputs[3], i32),
        e: parse_input!(inputs[4], i32),
      }
    }
    pub fn clamp_negative(self) -> MoleculeSet {
      MoleculeSet {
        a: cmp::max(0, self.a),
        b: cmp::max(0, self.b),
        c: cmp::max(0, self.c),
        d: cmp::max(0, self.d),
        e: cmp::max(0, self.e),
      }
    }
    pub fn abs(self) -> MoleculeSet {
      MoleculeSet {
        a: self.a.abs(),
        b: self.b.abs(),
        c: self.c.abs(),
        d: self.d.abs(),
        e: self.e.abs(),
      }
    }

    pub fn dificite(&self, other: &MoleculeSet) -> MoleculeSet {
      MoleculeSet {
        a: cmp::min(0, self.a - other.a),
        b: cmp::min(0, self.b - other.b),
        c: cmp::min(0, self.c - other.c),
        d: cmp::min(0, self.d - other.d),
        e: cmp::min(0, self.e - other.e),
      }
    }

    pub fn total(&self) -> i32 {
      self.a + self.b + self.c + self.d + self.e
    }

    pub fn can_substract(&self, other: &MoleculeSet) -> bool {
      self.a >= other.a
        && self.b >= other.b
        && self.c >= other.c
        && self.d >= other.d
        && self.e >= other.e
    }

    pub fn add(&mut self, name: &'static str, val: i32) {
      match name {
        crate::MOLECULE_A => self.a += val,
        crate::MOLECULE_B => self.b += val,
        crate::MOLECULE_C => self.c += val,
        crate::MOLECULE_D => self.d += val,
        crate::MOLECULE_E => self.e += val,
        _ => panic!(),
      };
    }

    pub fn sub(&mut self, name: &'static str, val: i32) {
      match name {
        crate::MOLECULE_A => self.a -= val,
        crate::MOLECULE_B => self.b -= val,
        crate::MOLECULE_C => self.c -= val,
        crate::MOLECULE_D => self.d -= val,
        crate::MOLECULE_E => self.e -= val,
        _ => panic!(),
      };
    }

    fn first_name(&self) -> &'static str {
      if self.a > 0 {
        return "A";
      }
      if self.b > 0 {
        return "B";
      }
      if self.c > 0 {
        return "C";
      }
      if self.d > 0 {
        return "D";
      }
      if self.e > 0 {
        return "E";
      }
      "U"
    }

    pub fn all_names(&self) -> Vec<&'static str> {
      let mut res: Vec<&'static str> = Vec::new();
      if self.a > 0 {
        res.push("A");
      }
      if self.b > 0 {
        res.push("B");
      }
      if self.c > 0 {
        res.push("C");
      }
      if self.d > 0 {
        res.push("D");
      }
      if self.e > 0 {
        res.push("E");
      }
      res
    }
  }

  impl Add for MoleculeSet {
    type Output = MoleculeSet;
    fn add(self, other: MoleculeSet) -> <Self as std::ops::Add<MoleculeSet>>::Output {
      MoleculeSet {
        a: self.a + other.a,
        b: self.b + other.b,
        c: self.c + other.c,
        d: self.d + other.d,
        e: self.e + other.e,
      }
    }
  }

  impl Sub for MoleculeSet {
    type Output = MoleculeSet;
    fn sub(self, other: MoleculeSet) -> <Self as std::ops::Sub<MoleculeSet>>::Output {
      MoleculeSet {
        a: self.a - other.a,
        b: self.b - other.b,
        c: self.c - other.c,
        d: self.d - other.d,
        e: self.e - other.e,
      }
    }
  }
}

mod input {
  use crate::entities::*;
  use crate::*;
  use std::io;
  pub fn parse_robots() -> Vec<Robot> {
    let mut robots: Vec<Robot> = Vec::new();
    for _ in 0..2 as usize {
      let mut input_line = String::new();
      io::stdin().read_line(&mut input_line).unwrap();
      let inputs = input_line.split(" ").collect::<Vec<_>>();
      let robot = Robot {
        target: parse_target(inputs[0].trim()),
        eta: parse_input!(inputs[1], i32),
        score: parse_input!(inputs[2], i32),
        storage: MoleculeSet::parse(&inputs[3..=7]),
        expertise: MoleculeSet::parse(&inputs[8..=12]),
      };
      robots.push(robot);
    }
    robots
  }

  fn parse_target<'a>(input: &'a str) -> &'static str {
    match input {
      LABORATORY => LABORATORY,
      DIAGNOSIS => DIAGNOSIS,
      SAMPLES => SAMPLES,
      MOLECULES => MOLECULES,
      START_POS => START_POS,
      _ => panic!("val : {}", input),
    }
  }

  pub fn parse_samples() -> Vec<Sample> {
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
        expertise_gain: parse_molecule(inputs[3].trim()),
        health: parse_input!(inputs[4], i32),
        cost: MoleculeSet::parse(&inputs[5..=9]),
        is_fake_diagnose: false,
      };

      samples.push(sample);
    }
    samples
  }

  pub fn parse_available() -> MoleculeSet {
    let mut input_line = String::new();
    io::stdin().read_line(&mut input_line).unwrap();
    let inputs = input_line.split(" ").collect::<Vec<_>>();
    MoleculeSet::parse(inputs.as_slice())
  }

  pub fn parse_projects() {
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
  }

  fn parse_molecule<'a>(input: &'a str) -> &'static str {
    match input {
      MOLECULE_A => MOLECULE_A,
      MOLECULE_B => MOLECULE_B,
      MOLECULE_C => MOLECULE_C,
      MOLECULE_D => MOLECULE_D,
      MOLECULE_E => MOLECULE_E,
      MOLECULE_0 => MOLECULE_0,
      _ => panic!("val : {}", input),
    }
  }
}

mod actions {
  use crate::entities::*;
  use crate::*;
  use std::ops::IndexMut;

  macro_rules! implement_name {
    ($n:literal) => {
      fn name(&self) -> &'static str {
        $n
      }
    };
  }

  fn type_name<T>(_: &T) -> &'static str {
    std::any::type_name::<T>()
  }

  pub trait Action {
    fn name(&self) -> &'static str;
    fn apply(&mut self, gs: &mut GameState);
    fn undo(&mut self, gs: &mut GameState);
    fn execute(&self);
  }

  /*
   * ActionGoTo
   */
  pub struct ActionGoTo {
    robot_idx: usize,
    initial_loc: &'static str,
    target_loc: &'static str,
  }

  impl ActionGoTo {
    pub fn new(robot_idx: usize, target_loc: &'static str) -> ActionGoTo {
      ActionGoTo {
        robot_idx: robot_idx,
        initial_loc: "",
        target_loc: target_loc,
      }
    }
  }

  impl Action for ActionGoTo {
    implement_name!("GoTo");
    fn apply(&mut self, gs: &mut GameState) {
      let mut robot = gs.robots.index_mut(self.robot_idx);
      self.initial_loc = robot.target;
      robot.target = self.target_loc;
      robot.eta = get_move_cost(self.initial_loc, robot.target) - 1;
    }
    fn undo(&mut self, gs: &mut GameState) {
      let mut robot = gs.robots.index_mut(self.robot_idx);
      robot.target = self.initial_loc;
      robot.eta = 0;
    }
    fn execute(&self) {
      println!("GOTO {}", self.target_loc);
    }
  }

  /*
   * ActionArrive
   */
  pub struct ActionArrive {
    robot_idx: usize,
  }

  impl ActionArrive {
    pub fn new(robot_idx: usize) -> ActionArrive {
      ActionArrive {
        robot_idx: robot_idx,
      }
    }
  }

  impl Action for ActionArrive {
    implement_name!("Arrive");
    fn apply(&mut self, gs: &mut GameState) {
      let mut robot = gs.robots.index_mut(self.robot_idx);
      robot.eta -= 1;
    }
    fn undo(&mut self, gs: &mut GameState) {
      let mut robot = gs.robots.index_mut(self.robot_idx);
      robot.eta += 1;
    }
    fn execute(&self) {
      println!("WAIT");
    }
  }

  /*
   * ActionWait
   */
  pub struct ActionWait {}

  impl ActionWait {
    pub fn new() -> ActionWait {
      ActionWait {}
    }
  }

  impl Action for ActionWait {
    implement_name!("Wait");
    fn apply(&mut self, _: &mut GameState) {}
    fn undo(&mut self, _: &mut GameState) {}
    fn execute(&self) {
      println!("WAIT");
    }
  }

  /*
   * ActionTakeSample
   */
  pub struct ActionTakeSample {
    robot_idx: usize,
    rank: i32,
    generated_sample_id: Option<i32>,
  }

  impl ActionTakeSample {
    pub fn new(robot_idx: usize, sample_count: i32, robot: &Robot) -> ActionTakeSample {
      ActionTakeSample {
        robot_idx: robot_idx,
        rank: ActionTakeSample::pick_rank(sample_count, robot),
        generated_sample_id: None,
      }
    }
    fn pick_rank(sample_count: i32, robot: &Robot) -> i32 {
      if robot.expertise.total() < 3 {
        return 1;
      }

      if robot.expertise.total() < 6 {
        return match sample_count {
          //2 => 1,
          _ => 2,
        };
      }

      //if robot.score < 100 {
      return match sample_count {
        2 => 2,
        _ => 3,
      };
      //}

      //3
    }
    unsafe fn next_id() -> i32 {
      let ret = FAKE_SAMPLE_ID;
      FAKE_SAMPLE_ID += 1;
      ret
    }
  }

  impl Action for ActionTakeSample {
    implement_name!("TakeSample");
    fn apply(&mut self, gs: &mut GameState) {
      let sample = Sample {
        sample_id: unsafe { ActionTakeSample::next_id() },
        carried_by: self.robot_idx as i32,
        rank: self.rank,
        expertise_gain: MOLECULE_0,
        health: 1,
        cost: MoleculeSet::new(-1),
        is_fake_diagnose: false,
      };
      self.generated_sample_id = Some(sample.sample_id);
      gs.samples.push(sample);
    }
    fn undo(&mut self, gs: &mut GameState) {
      if let Some(generated_sample_id) = self.generated_sample_id {
        gs.samples.retain(|x| x.sample_id != generated_sample_id);
      }
    }
    fn execute(&self) {
      println!("CONNECT {}", self.rank);
    }
  }

  /*
   * ActionDiagnoseSample
   */
  pub struct ActionDiagnoseSample {
    robot_idx: usize,
    diagnosed_sample_id: i32,
  }

  impl ActionDiagnoseSample {
    pub fn new(robot_idx: usize, diagnosed_sample_id: i32) -> ActionDiagnoseSample {
      ActionDiagnoseSample {
        robot_idx: robot_idx,
        diagnosed_sample_id: diagnosed_sample_id,
      }
    }

    fn get_sample_mut<'a>(&self, gs: &'a mut GameState) -> Option<&'a mut Sample> {
      let sample_idx = gs
        .samples
        .iter()
        .position(|x| x.sample_id == self.diagnosed_sample_id);
      if let Some(sample_idx) = sample_idx {
        return Some(gs.samples.index_mut(sample_idx));
      }
      None
    }
  }

  impl Action for ActionDiagnoseSample {
    implement_name!("Diagnose");
    fn apply(&mut self, gs: &mut GameState) {
      if let Some(sample) = self.get_sample_mut(gs) {
        sample.cost = MoleculeSet::new(5);
        sample.is_fake_diagnose = true;
      }
    }
    fn undo(&mut self, gs: &mut GameState) {
      if let Some(sample) = self.get_sample_mut(gs) {
        sample.cost = MoleculeSet::new(-1);
        sample.is_fake_diagnose = false;
      }
    }
    fn execute(&self) {
      println!("CONNECT {}", self.diagnosed_sample_id);
    }
  }

  /*
   * ActionTakeMolecule
   */
  pub struct ActionTakeMolecule {
    robot_idx: usize,
    molecule: &'static str,
  }

  impl ActionTakeMolecule {
    pub fn new(robot_idx: usize, molecule: &'static str) -> ActionTakeMolecule {
      ActionTakeMolecule {
        robot_idx: robot_idx,
        molecule: molecule,
      }
    }
  }

  impl Action for ActionTakeMolecule {
    implement_name!("TakeMole");
    fn apply(&mut self, gs: &mut GameState) {
      let robot = gs.robots.index_mut(self.robot_idx);
      robot.storage.add(self.molecule, 1);
      gs.available.sub(self.molecule, 1);
    }
    fn undo(&mut self, gs: &mut GameState) {
      let robot = gs.robots.index_mut(self.robot_idx);
      robot.storage.sub(self.molecule, 1);
      gs.available.add(self.molecule, 1);
    }
    fn execute(&self) {
      println!("CONNECT {}", self.molecule);
    }
  }

  /*
   * ActionTakeMolecule
   */
  pub struct ActionCompleteSample {
    robot_idx: usize,
    sample_id: i32,
    sample: Option<Sample>,
  }

  impl ActionCompleteSample {
    pub fn new(robot_idx: usize, sample_id: i32) -> ActionCompleteSample {
      ActionCompleteSample {
        robot_idx: robot_idx,
        sample_id: sample_id,
        sample: None,
      }
    }
  }

  impl Action for ActionCompleteSample {
    implement_name!("Complete");
    fn apply(&mut self, gs: &mut GameState) {
      let mut robot = gs.robots.index_mut(self.robot_idx);
      // todo expertise
      let sample_idx = gs
        .samples
        .iter()
        .position(|x| x.sample_id == self.sample_id);
      if let Some(sample_idx) = sample_idx {
        let sample = gs.samples.swap_remove(sample_idx);
        // todo consider last molecule take rule
        robot.score += sample.health;
        &robot.expertise.add(sample.expertise_gain, 1);
        gs.available = gs.available + sample.cost;
        self.sample = Some(sample);
      } else {
        panic!();
      }
    }
    fn undo(&mut self, gs: &mut GameState) {
      let mut robot = gs.robots.index_mut(self.robot_idx);
      // todo consider last molecule take rule
      if let Some(sample) = self.sample.take() {
        gs.available = gs.available - sample.cost;
        robot.score -= sample.health;
        robot.expertise.sub(sample.expertise_gain, 1);
        gs.samples.push(sample);
      }
    }
    fn execute(&self) {
      println!("CONNECT {}", self.sample_id);
    }
  }
}

mod minimax {
  use crate::actions::*;
  use crate::entities::*;
  use crate::*;
  use std::ops::IndexMut;

  struct Variation {
    score: f64,
    moves: [Vec<Box<dyn Action>>; 2],
    next_move: Option<[usize; 2]>,
  }

  impl Variation {
    fn new(score: f64, actions: Option<[Vec<Box<dyn Action>>; 2]>) -> Variation {
      Variation {
        score: score,
        moves: match actions {
          Some(x) => x,
          _ => [vec![], vec![]],
        },
        next_move: None,
      }
    }

    fn push_actions(&mut self, action_p0: Box<dyn Action>, action_p1: Box<dyn Action>) {
      self.moves[0].push(action_p0);
      self.moves[1].push(action_p1);
    }
  }

  pub fn decide_move(gs: &mut GameState) -> Option<Box<dyn Action>> {
    heuristic::evaluate(gs);

    let mut variation = minimax(gs, 0, 6, NEG_INFINITY, INFINITY);
    eprintln!("best {}", variation.score);
    print_variation(&variation.moves[0]);
    eprintln!("enemy");
    print_variation(&variation.moves[1]);
    let len = variation.moves[0].len();
    match len {
      0 => None,
      _ => Some(variation.moves[0].swap_remove(len - 1)),
    }
  }

  fn minimax<'a>(
    gs: &mut GameState,
    depth: i32,
    max_depth: i32,
    alpha: f64,
    beta: f64,
  ) -> Variation {
    unsafe { minimax_count_inc() };
    if depth == max_depth {
      return Variation::new(heuristic::evaluate(gs), None);
    }
    let mut alpha = alpha;
    let mut branch_p1 = heuristic::possible_moves(gs, 0);
    let mut branch_p2 = heuristic::possible_moves(gs, 1);

    let mut best_var = Variation::new(NEG_INFINITY, None);

    for mv_idx in 0..branch_p1.len() {
      let mv = branch_p1.index_mut(mv_idx);
      let mut best_var_2 = Variation::new(INFINITY, None);
      let mut local_beta = beta;
      for mv_2_idx in 0..branch_p2.len() {
        let mv_2 = branch_p2.index_mut(mv_2_idx);
        apply_action(gs, mv, mv_2, depth);
        let var = minimax(gs, depth + 1, max_depth, alpha, local_beta);
        undo_actions(gs, mv, mv_2, depth);
        local_beta = local_beta.min(var.score);
        if var.score < best_var_2.score {
          best_var_2 = var;
          best_var_2.next_move = Some([mv_idx, mv_2_idx]);
        }
        if local_beta <= alpha {
          break;
        }
      }
      alpha = alpha.max(best_var_2.score);
      if best_var_2.score > best_var.score {
        best_var = best_var_2;
      }
      if beta < alpha {
        break;
      }
    }

    if let Some(next_move) = best_var.next_move {
      let mv = branch_p1.swap_remove(next_move[0]);
      let mv_2 = branch_p2.swap_remove(next_move[1]);
      best_var.push_actions(mv, mv_2);
      best_var.next_move = None;
    } else {
      return Variation::new(heuristic::evaluate(gs), None);
    }

    best_var
  }

  fn print_variation(var: &Vec<Box<dyn Action>>) {
    for m in var.iter().rev() {
      eprintln!("{} ->", m.name());
    }
    eprintln!();
  }

  fn apply_action(
    gs: &mut GameState,
    mv: &mut Box<dyn Action>,
    mv_2: &mut Box<dyn Action>,
    depth: i32,
  ) {
    eprintln!("{} {} -> ", depth, mv.name());
    mv.apply(gs);
    mv_2.apply(gs);
  }
  fn undo_actions(
    gs: &mut GameState,
    mv: &mut Box<dyn Action>,
    mv_2: &mut Box<dyn Action>,
    depth: i32,
  ) {
    eprintln!("{} {} <- ", depth, mv.name());
    mv.undo(gs);
    mv_2.undo(gs);
  }
}

mod heuristic {
  use crate::actions::*;
  use crate::entities::*;
  use crate::*;
  use std::cmp;

  pub fn evaluate(gs: &GameState) -> f64 {
    let p_0 = evaluate_player(gs, 0);
    let p_1 = evaluate_player(gs, 1);
    let score = p_0 - p_1;
    eprintln!("p0: '{}'", p_0);
    //eprintln!("p0: {} p1:{} sc:{}", p_0, p_1, score);
    score
  }

  pub fn evaluate_player(gs: &GameState, robot_idx: i32) -> f64 {
    let mut score = 0f64;
    let expertise_coeff = 10f64;

    let robot = &gs.robots[robot_idx as usize];
    score += robot.score as f64;
    score += robot.expertise.total() as f64 * expertise_coeff;

    let mut robot_bank = robot.expertise + robot.storage;
    let mut field_bank = robot.expertise + robot.storage + gs.available;
    let mut sample_idx = 0;
    let mut samples_count = 0;
    for sample in sample_get_sorted(gs, robot_idx as usize) {
      samples_count += 1;
      let is_complete = robot_bank.can_substract(&sample.cost);
      let is_producable = field_bank.can_substract(&sample.cost);
      if is_complete {
        score += 0.85 * (sample.health as f64 + expertise_coeff);
        score -= 0.01 * (get_move_cost(robot.target, LABORATORY) + robot.eta) as f64;
      } else if is_producable {
        score += 0.5 * (sample.health as f64 + expertise_coeff);
      } else {
        score += 0.05 * (sample.health as f64 + expertise_coeff);
      }

      if !is_complete {
        let missing = (sample.cost - robot_bank).clamp_negative();
        let penalty = 1e-2f64 * 0.5f64.powi(sample_idx) * missing.total() as f64;
        score -= penalty;
      }

      if is_complete || is_producable {
        robot_bank = (robot_bank - sample.cost).clamp_negative();
        field_bank = (field_bank - sample.cost).clamp_negative();
      }
      sample_idx += 1;
    }

    for sample in gs
      .samples
      .iter()
      .filter(|&x| x.carried_by == robot_idx as i32 && (x.is_undiagnosed() || x.is_fake_diagnose))
    {
      samples_count += 1;
      if sample.is_undiagnosed() {
        score += 0.15 * (sample_get_min_score(sample.rank) as f64 + expertise_coeff);
        score -= 0.01 * (get_move_cost(robot.target, DIAGNOSIS) + robot.eta) as f64;
      } else if sample.is_diagnosed() {
        score += 0.175 * (sample_get_min_score(sample.rank) as f64 + expertise_coeff);
      }
    }

    score -=
      0.01 * (3 - samples_count) as f64 * (get_move_cost(robot.target, SAMPLES) + robot.eta) as f64;

    score
  }

  pub fn possible_moves(gs: &GameState, robot_idx: usize) -> Vec<Box<dyn Action>> {
    let robot = &gs.robots[robot_idx];
    let mut res: Vec<Box<dyn Action>> = Vec::new();
    if robot.eta < 0 {
      panic!();
    }
    if robot.eta != 0 {
      res.push(Box::new(ActionArrive::new(robot_idx)));
      return res;
    }
    match robot.target {
      SAMPLES => {
        let sample_count = sample_count_carry(gs, robot_idx);
        if sample_count < 3 {
          res.push(Box::new(ActionTakeSample::new(
            robot_idx,
            sample_count as i32,
            robot,
          )));
        }
        res.push(Box::new(ActionGoTo::new(robot_idx, DIAGNOSIS)));
      }
      DIAGNOSIS => {
        if let Some(x) = sample_first_undiagnosed(gs, robot_idx) {
          res.push(Box::new(ActionDiagnoseSample::new(robot_idx, x.sample_id)));
        } else {
          res.push(Box::new(ActionGoTo::new(robot_idx, MOLECULES)));
          res.push(Box::new(ActionGoTo::new(robot_idx, LABORATORY)));
        }
      }
      MOLECULES => {
        for molecule in molecule_required_all(gs, robot_idx) {
          res.push(Box::new(ActionTakeMolecule::new(robot_idx, molecule)));
        }
        res.push(Box::new(ActionGoTo::new(robot_idx, LABORATORY)));
        res.push(Box::new(ActionWait::new()));
      }
      LABORATORY => {
        if let Some(sample) = sample_first_completed(gs, robot_idx) {
          res.push(Box::new(ActionCompleteSample::new(
            robot_idx,
            sample.sample_id,
          )));
        } else {
          res.push(Box::new(ActionGoTo::new(robot_idx, MOLECULES)));
          res.push(Box::new(ActionGoTo::new(robot_idx, SAMPLES)));
        }
      }
      _ => {
        res.push(Box::new(ActionGoTo::new(robot_idx, SAMPLES)));
      }
    };
    res
  }

  fn sample_count_carry(gs: &GameState, robot_idx: usize) -> usize {
    gs.samples
      .iter()
      .filter(|&x| x.carried_by == robot_idx as i32)
      .count()
  }

  fn sample_first_undiagnosed(gs: &GameState, robot_idx: usize) -> Option<&Sample> {
    gs.samples
      .iter()
      .find(|&x| x.carried_by == robot_idx as i32 && x.is_undiagnosed())
  }

  fn sample_first_completed(gs: &GameState, robot_idx: usize) -> Option<&Sample> {
    let robot = &gs.robots[robot_idx];
    let optimal = gs
      .samples
      .iter()
      .filter(|&x| x.carried_by == robot_idx as i32 && x.is_diagnosed())
      .min_by_key(|&x| sample_weight(gs, robot, x));
    if let Some(sample) = optimal {
      if robot.missing_cost(&sample.cost).total() == 0 {
        return Some(sample);
      } else {
        return None;
      }
    }
    None
  }

  fn molecule_required_all(gs: &GameState, robot_idx: usize) -> Vec<&'static str> {
    //vec![MOLECULE_A, MOLECULE_B, MOLECULE_C, MOLECULE_D, MOLECULE_E]
    let robot = &gs.robots[robot_idx];
    let mut robot_bank = robot.storage + robot.expertise;
    let mut bank = robot.storage + robot.expertise + gs.available;
    let optimal = sample_get_sorted(gs, robot_idx);
    let target = optimal.iter().find(|&x| {
      let to_take = robot_bank.dificite(&x.cost).abs();
      let is_ready = to_take.total() == 0;
      let can_take =
        !is_ready && bank.can_substract(&x.cost) && (robot.storage + to_take).total() < 10;

      if is_ready && !can_take {
        bank = bank - x.cost;
        robot_bank = robot_bank - x.cost;
      }
      can_take
    });
    if let Some(target) = target {
      let to_take = robot_bank.dificite(&target.cost).abs();
      let mut best_val = 0;
      let mut best_name: Option<&str> = None;
      if to_take.a > best_val {
        best_val = to_take.a;
        best_name = Some(MOLECULE_A);
      }
      if to_take.b > best_val {
        best_val = to_take.b;
        best_name = Some(MOLECULE_B);
      }
      if to_take.c > best_val {
        best_val = to_take.c;
        best_name = Some(MOLECULE_C);
      }
      if to_take.d > best_val {
        best_val = to_take.d;
        best_name = Some(MOLECULE_D);
      }
      if to_take.e > best_val {
        best_val = to_take.e;
        best_name = Some(MOLECULE_E);
      }
      if let Some(best_name) = best_name {
        return vec![best_name];
      } else {
        panic!();
      }
    }
    Vec::new()
  }

  pub fn sample_get_sorted(gs: &GameState, robot_idx: usize) -> Vec<&Sample> {
    let robot = &gs.robots[robot_idx];
    let mut sorted = gs
      .samples
      .iter()
      .filter(|&x| x.carried_by == robot_idx as i32 && x.is_diagnosed() && !x.is_fake_diagnose)
      .collect::<Vec<_>>();
    sorted.sort_by_cached_key(|x| sample_weight(gs, robot, x));
    sorted
  }

  fn sample_weight(gs: &GameState, robot: &Robot, sample: &Sample) -> i64 {
    let to_take = (robot.storage + robot.expertise)
      .dificite(&sample.cost)
      .abs();
    let available = gs.available.can_substract(&to_take);
    let can_take = (robot.storage + to_take).total() < 10;
    let base = if to_take.total() == 0 {
      0
    } else if available && can_take {
      100
    } else if available {
      150
    } else {
      200
    };
    (base as f32 + to_take.total() as f32 * 1e3f32 * 10000f32) as i64
  }
}
