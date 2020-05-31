use std::fs::File;
use std::io::{self, BufRead};
use std::path::Path;
use imageproc::rect::Rect;
use image::{ImageBuffer, Rgb};


extern crate gio;
extern crate glib;
extern crate gtk;
extern crate gdk_pixbuf;
use gio::prelude::*;
use glib::clone;
use gtk::prelude::*;
use gtk::{
    ApplicationWindow, Image, Label, Builder, Button
};
use gdk_pixbuf::{Pixbuf};
use std::cell::RefCell;
use std::rc::Rc;

use std::env::args;

const SCALE: i32 = 30;

mod main;
use main::Context;

#[derive(Default)]
struct WndState {
    tick: i32,
    sim: SimModel,
}

fn build_ui(application: &gtk::Application) {
    let glade_src = include_str!("display.glade");
    let builder = Builder::new_from_string(glade_src);

    let window: ApplicationWindow = builder.get_object("window1").expect("Couldn't get window1");
    window.set_application(Some(application));

    let left_tick_btn: Button = builder.get_object("button1").expect("Couldn't get button1");
    let right_tick_btn: Button = builder.get_object("button2").expect("Couldn't get button2");

    let image: Image = builder.get_object("image1").expect("Couldn't get button2");


    let state = Rc::new(RefCell::new(WndState { tick: 0, sim: SimModel::new() }));
    

    right_tick_btn.connect_clicked(clone!(@weak image, @strong state => move |_| {
        let mut state = state.borrow_mut();

        state.tick += 1;

        let t = state.tick;
        state.sim.to_tick(t);
        build_tick_image(&state.sim);

        image.set_from_file("gray.png");
    }));

        
    left_tick_btn.connect_clicked(clone!(@weak image, @strong state => move |_| {
        let mut state = state.borrow_mut();
        state.tick -= 1;

        if state.tick < 0 {
            state.tick = 0;
        }

        let t = state.tick;
        state.sim.to_tick(t);
        build_tick_image(&state.sim);

        image.set_from_file("gray.png");
    }));


    window.show_all();
}

fn main() {
    let application =
        gtk::Application::new(Some("com.github.gtk-rs.examples.basic"), Default::default())
            .expect("Initialization failed...");

    application.connect_activate(|app| {
        build_ui(app);
    });

    application.run(&args().collect::<Vec<_>>());
}

fn build_tick_image(m: &SimModel) {
    let cx = &m.cx;
    let w = cx.map.width as i32 * SCALE;
    let h = cx.map.height as i32 * SCALE;
    let mut image = ImageBuffer::<Rgb<u8>, Vec<u8>>::new(w as u32, h as u32);

    for h in 0..cx.map.height {
        for w in 0..cx.map.width {
            let x = w as i32 * SCALE;
            let y = h as i32 * SCALE;
            let cell = &m.cx.map.get(w as i8, h as i8);
            if cell.wall {
                continue;
            }
            let color = match cell.pellet {
                0 => Rgb([255, 255, 255]),
                _ => Rgb([0, 255, 0]),
            };
            imageproc::drawing::draw_filled_rect_mut(&mut image, Rect::at(x,y).of_size(SCALE as u32, SCALE as u32), color);
        }
    }

    for pac in &m.cx.pacs {
        let x = pac.pos.x as i32 * SCALE;
        let y = pac.pos.y as i32 * SCALE;

        let color = match pac.is_mine {
            true =>Rgb([0,125,0]),
            false=>Rgb([255,0,0])
        };

        imageproc::drawing::draw_filled_circle_mut(&mut image, (x + SCALE/2, y + SCALE/2), SCALE/3, color)
    }
    
    image.save("gray.png").unwrap();
}


#[derive(Default)]
struct SimModel {
    tick: i32,
    cx: main::Context,
}

impl SimModel {
    fn new() -> Self {
        let mut m = SimModel::default();
        m.init();
        m
    }

    fn init(&mut self) {
        unsafe { fill_input() };
        self.tick = 0;
        self.cx = main::init();
    }

    fn to_tick(&mut self, i: i32) {
        if self.tick > i {
            self.init();
        }

        for _ in self.tick..i {
            main::read_tick(&mut self.cx);
        }
        self.tick = i;
    }
}

unsafe fn fill_input() {
    main::SIMULATE_INPUT = true;
    main::SIMULATE_INPUT_LINES.clear();
    main::SIMULATE_INPUT_IDX = 0;

    if let Ok(lines) = read_lines("game_input.txt") {
        for line in lines {
            if let Ok(ip) = line {
                main::SIMULATE_INPUT_LINES.push(ip)
            }
        }
    }
}

fn read_lines<P>(filename: P) -> io::Result<io::Lines<io::BufReader<File>>>
where P: AsRef<Path>, {
    let file = File::open(filename)?;
    Ok(io::BufReader::new(file).lines())
}



