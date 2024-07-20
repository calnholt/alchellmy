using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;

namespace Platformer2D
{
    public class MenuButton
    {
        private Texture2D _texture;
        private SpriteFont _font;
        private string _text;
        private Vector2 _position;
        private Rectangle _bounds;
        private Color _color;
        private Keys _hotKey;
        private Action _onClick;
        
        public string Code { get; set; }
        public string Name { get; set; }

        public MenuButton(Texture2D texture, SpriteFont font, string text, Vector2 position, Color color, Keys hotKey, Action onClick, string code, string name)
        {
            _texture = texture;
            _font = font;
            _text = text;
            _position = position;
            _color = color;
            _hotKey = hotKey;
            _onClick = onClick;

            Vector2 textSize = _font.MeasureString(text);
            _bounds = new Rectangle((int)_position.X, (int)_position.Y, (int)textSize.X + 20, (int)textSize.Y + 20);
            Name = name;
            Code = code;
        }

        public void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyboardState)
        {
            if (_bounds.Contains(mouseState.Position) && mouseState.LeftButton == ButtonState.Pressed)
            {
                _onClick.Invoke();
            }

            if (keyboardState.IsKeyDown(_hotKey))
            {
                _onClick.Invoke();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _bounds, _color);
            Vector2 textSize = _font.MeasureString(_text);
            spriteBatch.DrawString(_font, _text, _position + new Vector2(10, 10), Color.White);
        }
    }
}
