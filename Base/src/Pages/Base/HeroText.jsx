import React from 'react'

const HeroText = () => {
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-4 px-8 mt-20">
      <div className="flex flex-col justify-center">
        <h1 className="text-4xl font-bold">
          We specialize in UI/UX, Web Development, Digital Marketing.
        </h1>
        <p className="mt-4 text-lg text-gray-500">
          Lorem ipsum dolor sit amet, consectetur adipiscing elit. Quisque
          fringilla magna mauris. Nulla fermentum viverra sem eu rhoncus
          consequat varius nisi quis, posuere magna.
        </p>
        <div className="mt-4 flex align-center gap-6">
          <button className=" cursor-pointer bg-blue-600 text-white px-4 py-2 rounded-full hover:bg-blue-700 transition">
            Get Started
          </button>
          <div>
            <h1 className="text-lg font-semibold">Call us (0123) 456 – 789</h1>
            <p className="text-gray-500">For any question or concern</p>
          </div>
        </div>
      </div>
      <div className="flex "></div>
    </div>
  );
}

export default HeroText
