import React from 'react'
import Header from './Header'
import shape04 from "../../assets/images/shape-04.svg";
import hero from "../../assets/images/hero.png"
import HeroText from './HeroText';
const Index = () => {
  return (
    <div className="min-h-screen relative">
      <Header />
      <div className="absolute top-0 right-0 -z-10">
        <img src={shape04} alt="Quad Circle" className="w-[40rem] h-auto" />
      </div>
      <div className="absolute top-0 right-0 -z-10">
        <img src={hero} alt="Hero" className="w-[40rem] h-auto" />
      </div>
      <HeroText />
    </div>
  );
}

export default Index
