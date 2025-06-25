# ODL Board Game Web Client

This is the web client and development server for the ODL Board Game. Built with React and Vite, it provides the frontend interface for playing the board game in a web browser.

## Development

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run preview` - Preview production build

## TODO: Image Setup

When regenerating card images or other assets, remember to copy the following folders from the parent directory into `public/images/`:

- `blueprint-images/` - Blueprint card images
- `card-images/` - Generated card images  
- `card-images-raw/` - Raw card image files
- `card-layout-elements/` - Card layout element images

These copied images are used by the web client and are excluded from git via `.gitignore`.
