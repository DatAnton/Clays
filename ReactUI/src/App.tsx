import { useEffect } from 'react';
import './App.css'

function App() {

  useEffect(() => {
    fetch('/WeatherForecast')
      .then((response) => {
        if (!response.ok) {
          throw new Error('Error fetching weather forecasts');
        }
        return response;
      })
      .then((data) => {
        console.log('Weather Forecasts:', data);
      });
  }, []);
  
  return (
    <>
      Hello Clays!
    </>
  )
}

export default App
