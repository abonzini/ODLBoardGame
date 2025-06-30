# ODL Board Game Web Client

This is the web client and development server for the ODL Board Game. Built with React and Vite, it provides the frontend interface for playing the board game in a web browser.

## Development

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run preview` - Preview production build

## INSTRUCTIONS

Need to first generate all resources if not existing (i.e. when you just clone this)
Steps:
- Open **OdlEngine** VS project, build all, ensure all tests pass
- Generated executables should've been created
- Run **MassImageGenerator** and **TooltipGenerator** to generate card images and tooltips to be used in the UI

- After this, any `npm run dev` should copy them where it corresponds

These copied images are used by the web client and are excluded from git via `.gitignore`.
