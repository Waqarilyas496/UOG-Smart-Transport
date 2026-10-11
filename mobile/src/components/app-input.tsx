import { StyleSheet, TextInput, TextInputProps } from 'react-native';

export function AppInput(props: TextInputProps) {
  return (
    <TextInput
      placeholderTextColor="#6b7280"
      {...props}
      style={[styles.input, props.style]}
    />
  );
}

const styles = StyleSheet.create({
  input: {
    backgroundColor: '#ffffff',
    color: '#111827',
    borderWidth: 1,
    borderColor: '#d1d5db',
    borderRadius: 8,
    padding: 14,
    fontSize: 16,
  },
});