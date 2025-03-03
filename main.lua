local img
local lg = love.graphics
local t = {}
local dh, dw, ch, cw = 720, 368, 48, 32 --Deck/Card dimensions
local xo, yo = 0, 0 --XOffset, YOffset
local deck
function love.load()
  deck = love.graphics.newImage("deck.png")
  deck:setWrap("repeat", "repeat")

  love.window.setMode(640, 480, { vsync = false })
  lg.setDefaultFilter("nearest", "nearest")
  
  --Create deck
  local sci, cs = 1, "SPADES" --suit change iterator
  for i = 1, 52, 1 do
    local tt = {}
    tt.x = nil
    tt.y = nil
    tt.blit_location_x = i*cw+xo
    tt.blit_location_y = i*ch+yo
    tt.card_suit = cs
    t[i] = tt
    if sci >= 13 then
      cs = "HEARTS"
    end
  end
end

function love.update(dt)
  for y = 1, 52, 1 do
    print(t[y].blit_location_x)
  end
end

function love.draw()
  local i = 1
  for y = 1, 52, 1 do
    lg.draw(deck, lg.newQuad(t[y].blit_location_x, t[y].blit_location_y,cw,ch,dw,dh),1,1)
  end
end

function love.keypressed(k)
  if k == "escape" then love.event.quit() end
end
