# proiectMTP frontend

Plain React (Vite) frontend for the tutoring API.

## Run

The backend and frontend run separately.

1. Backend (from the project root):
   ```
   dotnet run --launch-profile http
   ```
   Serves the API at `http://localhost:5015`.

2. Frontend (from this `frontend/` folder):
   ```
   npm install
   npm run dev
   ```
   Serves the app at `http://localhost:5173`.

Open `http://localhost:5173` in a browser.

## Pages
- `/` — login / register
- `/dashboard` — list, create, edit, delete students
- `/students/:id` — sessions for a student: create, edit, delete, file upload/download

The API base URL is set in `src/api.js`.
