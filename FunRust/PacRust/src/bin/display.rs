use std::fs::File;
use std::io::{self, BufRead};
use std::path::Path;
use imageproc::drawing::draw_hollow_rect_mut;
use imageproc::rect::Rect;


use image::{ImageBuffer, Rgb};

const WIDTH: u32 = 300;
const HEIGHT: u32 = 300;

mod main;

fn main() {

    unsafe {
        fill_input();
    }


    main::main();

    let mut image = ImageBuffer::<Rgb<u8>, Vec<u8>>::new(WIDTH, HEIGHT);

    //let img = image::open("self.png").unwrap();
    // let mut gray = image.to_luma();
    let white = Rgb([255, 0, 255]);
    draw_hollow_rect_mut(&mut image, Rect::at(60, 10).of_size(20, 20), white);
    image.save("gray.png").unwrap();
}

unsafe fn fill_input() {
    main::SIMULATE_INPUT = true;
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