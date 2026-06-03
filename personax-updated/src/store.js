import { legacy_createStore as createStore } from 'redux'

const initialState = {
  sidebarShow: true,
  sidebarCollapsed: false,
  theme: 'light', // 'light' | 'dark' | 'auto'
}

const changeState = (state = initialState, { type, ...rest }) => {
  switch (type) {
    case 'set':
      return { ...state, ...rest }
    default:
      return state
  }
}

const store = createStore(changeState)
export default store
