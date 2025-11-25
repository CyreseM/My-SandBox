import './App.css'
import { BrowserRouter , Route,Routes} from 'react-router-dom';
import { Suspense } from 'react';
import Index from './Pages/Base/Index';
function App() {
  return (
    <BrowserRouter>
      <Suspense
        fallback={
          <div className="pt-3 text-center">
            <p>Loading...</p>
          </div>
        }
      >
        <Routes>
          <Route exact path="/" name="Home Page" element={<Index />} />
          {/* <Route
            exact
            path="/register"
            name="Register Page"
            element={<Register />}
          />
          <Route exact path="/404" name="Page 404" element={<Page404 />} />
          <Route exact path="/500" name="Page 500" element={<Page500 />} />
          <Route path="/callback" element={<Callback />} />
          <Route
            path="*"
            name="Home"
            element={
              <ProtectedRoute>
                <DefaultLayout />
              </ProtectedRoute>
            }
          />*/}
        </Routes>
      </Suspense>
    </BrowserRouter>
  );
}

export default App
