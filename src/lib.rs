use std::collections::HashSet;
use std::fs;
use std::fs::File;
use std::io::{BufReader, Read};
use std::path::Path;
use jpeg_decoder::Decoder as JpegDecoder;
use png::Decoder as PngDecoder;
use walkdir::WalkDir;

pub struct Config {
    pub origin_dir: String,
    pub destination_dir: String,
    pub ratio: f32,
}

impl Config {
    pub fn new(args: &[String]) -> Result<Config, &'static str> {
        if args.len() < 4 {
            return Err("Not enough arguments");
        }
        let origin = args[1].clone();
        let destination = args[2].clone();
        let ratio = args[3].clone().parse::<f32>().unwrap();
        Ok(Config {
            origin_dir: origin,
            destination_dir: destination,
            ratio,
        })
    }
}

pub fn run(config: Config) {
    let src_dir = config.origin_dir;
    let dest_dir = config.destination_dir;
    let aspect_ratio_threshold = config.ratio;
    check_exists(&dest_dir);

    let allowed_extensions: HashSet<&str> = ["jpg", "jpeg", "png"].iter().cloned().collect();

    for entry in WalkDir::new(src_dir).into_iter().filter_map(|e| e.ok()) {
        let path = entry.path();
        if let Some(ext) = path.extension().and_then(|ext| ext.to_str()).map(|ext| ext.to_lowercase()) {
            if allowed_extensions.contains(ext.as_str()) {
                println!("Processing file: {:?}", path);
                let file = match File::open(path) {
                    Ok(file) => file,
                    Err(_) => {
                        eprintln!("Failed to open image file: {:?}", path);
                        continue;
                    }
                };

                let dimensions = match ext.as_str() {
                    "jpg" | "jpeg" => get_jpeg_metadata(file),
                    "png" => get_png_metadata(file),
                    _ => {
                        eprintln!("Unsupported file format: {:?}", path);
                        continue;
                    }
                };

                match dimensions {
                    Some((width, height)) => {
                        let aspect_ratio = width / height;
                        println!("Width: {}, Height: {}, Aspect Ratio: {}", width, height, aspect_ratio);

                        if aspect_ratio > aspect_ratio_threshold {
                            let file_name = path.file_name().unwrap();
                            let dest_path = Path::new(&dest_dir).join(file_name);
                            if let Err(_) = fs::rename(path, dest_path) {
                                eprintln!("Failed to move file: {:?}", path);
                            } else {
                                println!("Moved: {:?}", file_name);
                            }
                        }
                    }
                    None => {
                        eprintln!("Failed to read image metadata for: {:?}", path);
                    }
                }
            }
        }
    }
}

fn check_exists(dir: &str) {
    if !Path::new(dir).exists() {
        fs::create_dir(dir).expect("Failed to create destination directory");
    }
}

fn get_jpeg_metadata(mut file: File) -> Option<(f32, f32)> {
    let mut decoder = JpegDecoder::new(BufReader::new(file));
    if let Err(_) = decoder.read_info() {
        return None;
    }
    let metadata = decoder.info()?;
    Some((metadata.width as f32, metadata.height as f32))
}

fn get_png_metadata(file: File) -> Option<(f32, f32)> {
    let decoder = PngDecoder::new(BufReader::new(file));
    match decoder.read_info() {
        Ok(reader) => {
            let info = reader.info(); // 获取图像的元数据
            Some((info.width as f32, info.height as f32))
        }
        Err(_) => None,
    }
}